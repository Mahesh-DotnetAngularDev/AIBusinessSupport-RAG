# AIBusinessSupport-RAG

An AI-assisted business support application built with ASP.NET Core, Angular, Groq, and PostgreSQL with pgvector. It combines business-data tools with retrieval over uploaded PDF text, and streams answers into an Angular chat interface.

> This README describes the code currently in this repository. PostgreSQL and the embedding HTTP service are external prerequisites; Aspire does not create a database or embedding service here.

## Features

- .NET 10 ASP.NET Core Web API with customer, order, support-ticket, chat, and document endpoints.
- Angular 19 chat UI with streamed assistant responses, Markdown rendering, and a Knowledge Base list showing document chunk counts.
- Groq chat completion through the OpenAI-compatible .NET client and `Microsoft.Extensions.AI`.
- AI business tools for customer-name search, customer orders, and orders by status.
- PDF upload and text extraction with PdfPig; plain text ingestion is also available.
- Character-based text chunking (500 characters with 100 characters of overlap).
- Embedding requests to a separate HTTP service; the API validates that each embedding has 384 values.
- PostgreSQL/pgvector storage and nearest-vector retrieval of up to five chunks per question.
- .NET Aspire AppHost to orchestrate the API and Angular frontend.

## Architecture

### Document ingestion

1. The Angular composer uploads a PDF to `POST /api/AI/documents/upload-pdf`. The API accepts PDFs up to 5 MB, checks the `.pdf` extension and `%PDF-` file signature, and extracts page text with PdfPig. The alternate `POST /api/AI/documents/ingest` endpoint accepts text directly.
2. `TextChunkingService` splits the extracted text into 500-character chunks with 100 characters of overlap.
3. `EmbeddingService` sends each chunk to `POST http://localhost:8081/embed` as `{"text":"..."}`. It expects `{"embedding":[...]}` with exactly 384 numeric values.
4. `DocumentIngestionService` stores each chunk and vector in the PostgreSQL `document_chunks` table. Re-ingesting a name replaces its prior chunks in a database transaction.
5. The Knowledge Base calls `GET /api/AI/documents` to group stored chunks by document name, show counts, and provide a delete action.

### Question and response

1. The Angular client posts the conversation ID and question to `POST /api/AI/chat/stream` and displays the returned text stream as it arrives.
2. `AIService` embeds the question, then `DocumentSearchRepository` orders stored vectors by pgvector cosine distance (`<=>`) and returns up to five chunk texts.
3. The retrieved text is included as reference context in the prompt. The prompt instructs the model not to invent document facts and to say when the available context does not answer a policy question. There is no separate relevance threshold or reranker in the current search query.
4. When business data is needed, Groq can invoke the registered `SearchCustomersByName`, `GetOrdersByCustomer`, and `GetOrderByStatus` tools. The service sends tool results back to the model before completing the response.
5. The API streams the final assistant text as `text/plain`; the Angular UI renders assistant text as sanitized Markdown. User messages remain plain text.

Conversation messages are held in the API process's in-memory `ConversationStore`, keyed by `ConversationId`. They are lost on restart and are not stored in PostgreSQL.

## Technology stack

| Area | Technologies |
|---|---|
| API | .NET 10, ASP.NET Core controllers, EF Core 10, Npgsql |
| AI | Groq OpenAI-compatible endpoint, OpenAI .NET SDK, `Microsoft.Extensions.AI` |
| RAG/vector storage | PostgreSQL, pgvector, 384-dimensional vectors |
| PDF and text | PdfPig, 500-character chunks with 100-character overlap |
| Embeddings | External HTTP service at `http://localhost:8081`; `/embed` request/response contract described above |
| Frontend | Angular 19, TypeScript, RxJS, `ngx-markdown` |
| Orchestration/telemetry | .NET Aspire 13.6, ServiceDefaults, OpenTelemetry |

The API code does not select or bundle an embedding model. `all-MiniLM-L6-v2` is compatible with the expected 384-dimensional output, but the model must be selected/configured in the separately run embedding service; that service is not included in this repository.

## Repository structure

```text
.
├── AIBusinessSupport.slnx
├── AIBusinessSupport.API/
│   ├── AI/
│   │   ├── AIService.cs
│   │   ├── IAIService.cs
│   │   └── Tools/BusinessTools.cs
│   ├── Controllers/
│   │   ├── AIController.cs
│   │   ├── CustomersController.cs
│   │   ├── OrderController.cs
│   │   ├── SupportTicketsController.cs
│   │   └── WeatherForecastController.cs
│   ├── Data/                 # EF Core context and sample-data seeder
│   ├── DTOs/
│   ├── Migrations/           # Relational EF Core migrations
│   ├── Models/
│   ├── Services/
│   │   ├── Conversations/    # In-memory conversation store
│   │   ├── Embeedings/       # Chunking and embedding client (folder spelling as in repo)
│   │   ├── VectorStore/      # Ingestion, search, SQL repositories, connection factory
│   │   └── PdfTextExtractionService.cs
│   ├── Program.cs
│   └── AIBusinessSupport.API.csproj
├── AIBusinessSupport.AppHost/
│   ├── AppHost.cs
│   └── AIBusinessSupport.AppHost.csproj
├── AIBusinessSupport.ServiceDefaults/
│   ├── Extensions.cs
│   └── AIBusinessSupport.ServiceDefaults.csproj
├── AIBusinessSupport.Web/
│   ├── proxy.conf.cjs
│   ├── package.json
│   └── src/app/chat/         # Chat and Knowledge Base UI
└── AIBusinessSupportAPI/    # Separate minimal/template API project; not in the solution
```

The solution file includes `AIBusinessSupport.API`, `AIBusinessSupport.AppHost`, and `AIBusinessSupport.ServiceDefaults`. The separate `AIBusinessSupportAPI` directory is not included in the solution and does not contain the RAG implementation.

## Prerequisites

- .NET 10 SDK.
- Node.js and npm compatible with Angular 19 (Node.js 20 is suitable).
- PostgreSQL with the pgvector extension installed and available to the database user.
- A running embedding service reachable by the API at `http://localhost:8081`, implementing `POST /embed` and returning 384-dimensional vectors.
- A Groq API key with access to the model configured in `AIBusinessSupport.API/Program.cs` (`openai/gpt-oss-20b`).
- The `dotnet-ef` tool if you need to apply the EF Core migration.

## Configuration

The checked-in API and AppHost settings contain logging configuration only. Supply connection strings and the Groq key locally with .NET User Secrets or environment variables; do not commit secrets.

The API reads these exact configuration keys:

| Key | Purpose |
|---|---|
| `ConnectionStrings:BusinessDb` | EF Core business data and startup seeding |
| `ConnectionStrings:pgvector` | Raw Npgsql connection used by document ingestion, listing, deletion, and vector search |
| `Groq:ApiKey` | Groq API credential |

For example, from the repository root (replace every placeholder):

```bash
dotnet user-secrets set "ConnectionStrings:BusinessDb" "Host=localhost;Port=5432;Database=business_support;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD" --project AIBusinessSupport.API/AIBusinessSupport.API.csproj
dotnet user-secrets set "ConnectionStrings:pgvector" "Host=localhost;Port=5432;Database=business_support;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD" --project AIBusinessSupport.API/AIBusinessSupport.API.csproj
dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY" --project AIBusinessSupport.API/AIBusinessSupport.API.csproj
```

The two database keys may point to the same database if that database has both the EF Core schema and the vector schema. The embedding URL is currently hard-coded in `Program.cs`; changing it requires an application-code change.

### Aspire connection-string note

`AppHost.cs` declares an external connection string named `pgvector` and currently calls `.WithReference(pgvector)` on the **web** resource. The API independently reads `ConnectionStrings:pgvector`, but the AppHost does not reference that resource on the API. For an Aspire launch, configure the named connection string for the AppHost and also provide it to the API through the API's User Secrets/environment until this wiring is corrected. For example:

```bash
dotnet user-secrets set "ConnectionStrings:pgvector" "Host=localhost;Port=5432;Database=business_support;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD" --project AIBusinessSupport.AppHost/AIBusinessSupport.AppHost.csproj
```

The equivalent environment-variable names are `ConnectionStrings__BusinessDb`, `ConnectionStrings__pgvector`, and `Groq__ApiKey`. The external embedding service must also be available from the API process on port `8081`.

## Database initialization

The EF Core migration creates the business tables (`Customers`, `Products`, `Orders`, `OrderItems`, `SupportTickets`, and `Documents`). The API seeds sample business records on startup, but it does not apply migrations automatically. Configure `BusinessDb`, then run from the repository root:

```bash
dotnet ef database update --project AIBusinessSupport.API/AIBusinessSupport.API.csproj --startup-project AIBusinessSupport.API/AIBusinessSupport.API.csproj
```

The current migration does **not** create the pgvector extension or `document_chunks` table, and the repository has no SQL initialization script for them. Create these in the database addressed by `ConnectionStrings:pgvector` before using chat/document features. The following illustrates the columns expected by the repository's SQL; adapt it to your database/schema policy:

```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS document_chunks (
		document_name text NOT NULL,
		chunk_text text NOT NULL,
		embedding vector(384),
		created_at timestamptz NOT NULL DEFAULT now()
);
```

The external embedding service contract is:

```http
POST http://localhost:8081/embed
Content-Type: application/json

{"text":"Text to embed"}
```

```json
{"embedding":[0.01, -0.02, 0.03]}
```

The abbreviated array above is illustrative only: the actual response must contain exactly 384 numeric values. The repository does not include or start this service.

## Run the application

### With Aspire

After configuring the required secrets, database schemas, and embedding service, run:

```bash
dotnet run --project AIBusinessSupport.AppHost/AIBusinessSupport.AppHost.csproj
```

Open the Aspire dashboard and use its resource links for the API and Angular app. Aspire assigns the browser-facing frontend endpoint; Angular listens on target port `4200`. The AppHost waits for the API before starting the web resource.

### Run API and Angular separately

Start the API in one terminal:

```bash
dotnet run --project AIBusinessSupport.API/AIBusinessSupport.API.csproj
```

The launch settings define HTTP port `5090` and HTTPS port `7089`. In Development, Swagger UI is available at `/swagger` (for example, `http://localhost:5090/swagger`).

Start Angular in another terminal:

```bash
npm install --prefix AIBusinessSupport.Web
npm start --prefix AIBusinessSupport.Web
```

The standalone Angular dev server listens on `http://localhost:4200`. Its proxy uses Aspire's discovered API URL when available and falls back to `http://localhost:5090` for a standalone API launch.

## API reference

Routes below are the routes defined by the controllers. ASP.NET Core route matching is case-insensitive, but examples retain the controller token casing from source.

### AI chat and documents

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/AI/chat` | Non-streaming chat; returns JSON `{ "response": "..." }`. |
| `POST` | `/api/AI/chat/stream` | Streams assistant text as `text/plain`; this is not an SSE event stream. |
| `POST` | `/api/AI/documents/ingest` | Ingests supplied text under a document name. |
| `POST` | `/api/AI/documents/upload-pdf` | Extracts and ingests an uploaded PDF. Multipart field name: `file`; maximum size: 5 MB. |
| `GET` | `/api/AI/documents` | Lists document names, chunk counts, and upload timestamps. |
| `DELETE` | `/api/AI/documents/{documentName}` | Deletes all vector chunks for a document name; returns `404` if none were deleted. |

Both chat endpoints accept a conversation ID and message:

```json
{
	"conversationId": "00000000-0000-0000-0000-000000000001",
	"message": "What is the refund processing time?"
}
```

For `/api/AI/chat`, an example response is:

```json
{"response":"The available documents say ..."}
```

For streaming chat, the body is the assistant text in successive chunks. Example request:

```bash
curl --no-buffer -X POST http://localhost:5090/api/AI/chat/stream \
	-H "Content-Type: application/json" \
	-d '{"conversationId":"00000000-0000-0000-0000-000000000001","message":"Summarize the shipping policy."}'
```

Text ingestion accepts:

```http
POST /api/AI/documents/ingest
Content-Type: application/json
```

```json
{
	"documentName": "business-policy.txt",
	"text": "Policy text to add to the knowledge base."
}
```

The response includes `message`, `documentName`, and `chunksCreated`. PDF upload example:

```bash
curl -X POST http://localhost:5090/api/AI/documents/upload-pdf \
	-F "file=@./docs/business-policy.pdf"
```

The document list has this shape:

```json
[
	{
		"documentName": "business-policy.pdf",
		"chunkCount": 8,
		"uploadedAt": "2026-01-01T12:00:00Z"
	}
]
```

Delete a document and its chunks by name:

```bash
curl -X DELETE http://localhost:5090/api/AI/documents/business-policy.pdf
```

### Business data and sample endpoint

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/Customers` | List customers. |
| `GET` | `/api/Customers/{id}` | Get a customer by ID. |
| `GET` | `/api/Customers/email/{email}` | Get a customer by email. |
| `GET` | `/api/Order` | List orders. |
| `GET` | `/api/Order/{id}` | Get an order by ID. |
| `GET` | `/api/Order/customer/{customerId}` | List orders for a customer. |
| `GET` | `/api/Order/status/{status}` | List orders by status. |
| `GET` | `/api/SupportTickets` | List support tickets. |
| `GET` | `/api/SupportTickets/{id}` | Get a support ticket by ID. |
| `GET` | `/api/SupportTickets/customer/{customerId}` | List tickets for a customer. |
| `GET` | `/api/SupportTickets/status/{status}` | List tickets by status. |
| `GET` | `/api/SupportTickets/priority/{priority}` | List tickets by priority. |
| `GET` | `/WeatherForecast` | Development/template weather forecast endpoint. |

Example business data request:

```bash
curl http://localhost:5090/api/Order/customer/12
```

The order response includes order ID, customer, date, status, total amount, and item details. These endpoints are read-only. Customer-name search is currently exposed as an AI business tool, not as a customer search HTTP route.

## Use the Knowledge Base and test RAG

1. Start the database and the embedding service, then launch the API and Angular app.
2. In the chat composer, choose **Upload PDF** and select a readable PDF no larger than 5 MB. The UI reports the document name and number of chunks created.
3. View the **Knowledge Base** section in the sidebar to see document names and chunk counts. Use the delete button next to a document to remove its stored chunks. The original PDF file is not retained by this implementation.
4. Ask a question whose answer appears in an uploaded policy document. For example: “What is the refund processing time?” or “How long does shipping take?”
5. Test the fallback with a question absent from the uploaded material, such as “What is the international warranty period?” If no relevant policy information is present, the prompt instructs the assistant to say the available documents do not provide enough information.

The Angular Knowledge Base loads the list on startup and refreshes after upload or deletion. It displays stored chunks, not original PDF files.

## Troubleshooting

| Symptom | Checks |
|---|---|
| API fails during startup or database calls fail | Confirm `ConnectionStrings:BusinessDb` is set and reachable. Run the EF migration before the API starts; startup seeding expects the relational tables to exist. |
| Document/chat request fails during embedding | Confirm the embedding process is reachable at `http://localhost:8081/embed`, accepts `{"text":"..."}`, and returns an `embedding` array of exactly 384 numbers. The URL is hard-coded in `Program.cs`. |
| PostgreSQL reports missing `vector` type or `document_chunks` relation | Install/enable the pgvector extension and create the `document_chunks` schema in the database addressed by `ConnectionStrings:pgvector`; these are not in the checked-in EF migration. |
| Aspire starts Angular but RAG database calls fail | Configure `ConnectionStrings:pgvector` for both AppHost (it declares the external connection string) and the API. Current AppHost wiring references the connection string on the web resource, not the API. |
| PDF upload is rejected or creates no chunks | Check the 5 MB limit, `.pdf` extension, valid `%PDF-` signature, and that the PDF contains extractable text. Image-only/scanned PDFs need OCR, which is not implemented. |
| The assistant cannot answer a document question | Confirm the document appears in the Knowledge Base, that chunk creation succeeded, and that the embedding service and pgvector table are available. Ask using terms that occur in the document. Retrieval currently returns the five closest chunks without a relevance cutoff or citations. |
| Angular displays its connection error | Check that the API is healthy and that the Angular proxy targets the correct API address. For standalone runs it falls back to port `5090`; Aspire supplies a discovered API URL. |

## Limitations and unverified setup

- Scanned/image-only PDFs may require OCR. OCR is not implemented; PdfPig extracts embedded text only.
- The embedding service is not part of this repository. Its model is not configured here; only the local base URL, `/embed` route, and 384-value response are enforced. Configure the external service to use `all-MiniLM-L6-v2` if that is the intended model.
- The pgvector extension and `document_chunks` table have no checked-in migration or SQL bootstrap file. The SQL above reflects the columns used by current queries and must be validated against the chosen PostgreSQL/pgvector deployment.
- AppHost declares an external `pgvector` connection string but does not add a PostgreSQL server/container. Its current reference is attached to the Angular resource rather than the API, while the API reads the same-named connection string itself.
- RAG uses up to five nearest chunks without a distance threshold, reranking, or source citations. The assistant is prompted to abstain when context does not answer the question, but retrieval itself does not filter weak matches.
- Conversation history is held in a process-local dictionary and is not durable or shared across API instances.
- The application does not retain uploaded PDF files; only extracted text chunks and vectors are stored.
- No authentication or authorization is configured on these API endpoints in the inspected code. Do not expose the sample application publicly without adding appropriate access controls.

## Roadmap

- Add migrations/bootstrap tooling for the vector extension and chunk table.
- Wire the external vector connection and embedding service through Aspire and health checks; make the embedding URL/model configurable.
- Add retrieval relevance thresholds, source references, and evaluation tests for grounded answers.
- Add OCR for scanned PDFs, durable conversation storage, and authentication/authorization before production deployment.

## Suggested GitHub topics

`aspnet-core` `dotnet` `angular` `rag` `retrieval-augmented-generation` `pgvector` `postgresql` `groq` `pdf-processing` `embeddings` `aspire` `generative-ai`