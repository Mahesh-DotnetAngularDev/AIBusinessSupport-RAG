const apiTarget =
  process.env.services__aibusinesssupportapi__http__0 ?? 'http://localhost:5090';

module.exports = {
  '/api/**': {
    target: apiTarget,
    secure: false,
    changeOrigin: true
  }
};