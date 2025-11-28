require("dotenv").config();
const express = require("express");
const cors = require("cors");
const LogService = require("./services/LogService");

const app = express();
const port = process.env.PORT || 8080;

app.use(cors());
app.use(express.json());

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
