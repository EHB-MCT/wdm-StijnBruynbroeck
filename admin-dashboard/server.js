const express = require('express');
const path = require('path');
const cors = require('cors');

const app = express();
const port = process.env.ADMIN_PORT || 3001;

// Enable CORS
app.use(cors({
    origin: ['http://localhost:8080', 'http://127.0.0.1:8080', '*'],
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));

// Serve static files from admin-dashboard directory
app.use(express.static(path.join(__dirname)));

// Route for admin dashboard
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'Admin Dashboard is running', port });
});

app.listen(port, () => {
    console.log(`🎮 Admin Dashboard running on http://localhost:${port}`);
    console.log(`📊 Access dashboard at: http://localhost:${port}`);
});

module.exports = app;