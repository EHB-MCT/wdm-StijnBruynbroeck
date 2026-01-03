require("dotenv").config();
const express = require("express");
const cors = require("cors");
const LogService = require("./services/LogService");

const app = express();
const port = process.env.PORT || 8080;

app.use(cors({
    origin: ['http://localhost:3000', 'http://127.0.0.1:3000', 'http://localhost:3001', 'http://127.0.0.1:3001', '*'],
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));

// Debug middleware to log ALL requests
app.use((req, res, next) => {
    if (req.method === 'POST' && req.path === '/api/log') {
        console.log('=== INCOMING POST REQUEST ===');
        console.log('Method:', req.method);
        console.log('Path:', req.path);
        console.log('Content-Type:', req.get('Content-Type'));
        console.log('Raw body (before parsing):', req.body);
    }
    next();
});

app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

app.use("/api", require("./routes/logRoutes"));

app.get("/", (req, res) => {
	res.send("Backend is Online.");
});

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const startServer = async () => {
	let retries = 5;

	while (retries > 0) {
		try {
			console.log(
				` Trying to connect with database... (Tries over: ${retries})`
			);

			await LogService.initializeTables();

			console.log(" Database tables initialized! Starting Web Server...");

			app.listen(port, () => {
				console.log(` Backend is live and is listening on port ${port}`);
			});

			return;
		} catch (err) {
			console.error(" Database connection failed:", err.message);
			retries -= 1;
			console.log(" Waiting for the next try...");
			await sleep(5000);
		}
	}

	console.error(
		" Couldn't make a connection with the server after 5 tries. Server stops."
	);
	process.exit(1);
};

startServer();
