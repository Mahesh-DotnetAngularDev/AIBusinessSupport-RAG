import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MarkdownComponent } from 'ngx-markdown';
import { firstValueFrom } from 'rxjs';

interface ChatMessage {
    role: 'user' | 'assistant';
    text: string;
}
interface KnowledgeDocument {
    documentName: string;
    chunkCount: number;
    uploadedAt: string;
}
interface ChatResponse {
    response: string;
}

@Component({
    selector: 'app-chat',
    imports: [FormsModule, MarkdownComponent],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.css'
})
export class ChatComponent {
    private readonly http = inject(HttpClient);

    conversationId = crypto.randomUUID();

    @ViewChild('conversation') private conversation?: ElementRef<HTMLElement>;

    readonly suggestions = [
        'Show orders for customer 12',
        'Which orders are pending?',
        'Find a customer by name'
    ];

    messages: ChatMessage[] = [];
    draft = '';
    isSending = false;
    isUploadingPdf = false;
    uploadMessage = '';
    documents: KnowledgeDocument[] = [];
    isDeletingDocument: string | null = null;
isLoadingDocuments = false;

ngOnInit(): void {
    void this.loadDocuments();
}
    async sendMessage(message = this.draft): Promise<void> {
        const text = message.trim();

        if (!text || this.isSending) {
            return;
        }

        this.messages = [...this.messages, { role: 'user', text }];
        this.draft = '';
        this.isSending = true;
        this.scrollToLatest();

        try {
            const response = await fetch('/api/AI/chat/stream', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    conversationId: this.conversationId,
                    message: text
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }

            const reader = response.body?.getReader();

            if (!reader) {
                throw new Error('Streaming response body is unavailable.');
            }

            const decoder = new TextDecoder();

            this.messages = [
                ...this.messages,
                { role: 'assistant', text: '' }
            ];

            const assistantIndex = this.messages.length - 1;

            while (true) {
                const { done, value } = await reader.read();

                if (done) {
                    break;
                }

                const chunk = decoder.decode(value, { stream: true });

                this.messages[assistantIndex] = {
                    ...this.messages[assistantIndex],
                    text: this.messages[assistantIndex].text + chunk
                };

                this.scrollToLatest();
            }
        } catch {
            this.messages = [
                ...this.messages,
                {
                    role: 'assistant',
                    text: "I couldn't reach the assistant. Check that the API is running, then try again."
                }
            ];
        } finally {
            this.isSending = false;
            this.scrollToLatest();
        }
    }
    async onPdfSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
        return;
    }

    this.uploadMessage = '';

    if (!file.name.toLowerCase().endsWith('.pdf')) {
        this.uploadMessage = 'Please select a PDF file.';
        input.value = '';
        return;
    }

    if (file.size > 5 * 1024 * 1024) {
        this.uploadMessage = 'PDF size must not exceed 5 MB.';
        input.value = '';
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    this.isUploadingPdf = true;
    this.uploadMessage = 'Uploading and processing PDF...';

    try {
        const result = await firstValueFrom(
            this.http.post<{
                message: string;
                documentName: string;
                chunksCreated: number;
            }>('/api/AI/documents/upload-pdf', formData)
        );

        this.uploadMessage =
            `${result.documentName} uploaded successfully. ` +
            `${result.chunksCreated} chunks created.`;
            await this.loadDocuments();
    } catch {
        this.uploadMessage =
            'PDF upload failed. Check that the API is running and try again.';
    } finally {
        this.isUploadingPdf = false;
        input.value = '';
    }
}
async loadDocuments(): Promise<void> {
    this.isLoadingDocuments = true;

    try {
        this.documents = await firstValueFrom(
            this.http.get<KnowledgeDocument[]>('/api/AI/documents')
        );
    } catch {
        this.uploadMessage = 'Unable to load uploaded documents.';
    } finally {
        this.isLoadingDocuments = false;
    }
}
    onComposerKeydown(event: KeyboardEvent): void {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            void this.sendMessage();
        }
    }

    startNewChat(): void {
        if (this.isSending) {
            return;
        }

        this.messages = [];
        this.draft = '';
    }

    useSuggestion(suggestion: string): void {
        this.draft = suggestion;
    }

    private scrollToLatest(): void {
        requestAnimationFrame(() => {
            const element = this.conversation?.nativeElement;

            if (element) {
                element.scrollTop = element.scrollHeight;
            }
        });
    }
    
async deleteDocument(documentName: string): Promise<void> {
    if (this.isDeletingDocument) {
        return;
    }

    const confirmed = window.confirm(
        `Are you sure you want to delete "${documentName}"? ` +
        'Its chunks will be removed from the knowledge base.'
    );

    if (!confirmed) {
        return;
    }

    this.isDeletingDocument = documentName;
    this.uploadMessage = '';

    try {
        await firstValueFrom(
            this.http.delete(
                `/api/AI/documents/${encodeURIComponent(documentName)}`
            )
        );

        this.documents = this.documents.filter(
            document => document.documentName !== documentName
        );

        this.uploadMessage = `"${documentName}" deleted successfully.`;
    } catch {
        this.uploadMessage = `Failed to delete "${documentName}". Please try again.`;
        await this.loadDocuments();
    } finally {
        this.isDeletingDocument = null;
    }
}

}