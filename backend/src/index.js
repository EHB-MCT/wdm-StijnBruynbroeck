require("dotenv").config();
const express = require("express");
const cors = require("cors");
const { Pool } = require("pg");

const app = express();
const port = process.env.PORT || 8080;

app.use(cors());
app.use(express.json());

const pool = new Pool({
	connectionString: process.env.DATABASE_URL,
});

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const startServer = async () => {
	let retries = 5;

	while (retries > 0) {
		try {
			console.log(
				` Poging tot verbinden met database... (pogingen over: ${retries})`
			);

			await pool.query(`
                CREATE TABLE IF NOT EXISTS users (
                    uid VARCHAR(255) PRIMARY KEY,
                    platform VARCHAR(50),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            `);

			await pool.query(`
                CREATE TABLE IF NOT EXISTS game_actions (
                    id SERIAL PRIMARY KEY,
                    user_uid VARCHAR(255),
                    action_type VARCHAR(50),
                    action_data JSONB,
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            `);

			console.log("✅ Database tables initialized! Starting Web Server...");

			app.listen(port, () => {
				console.log(`🚀 Backend is live en luistert op poort ${port}`);
			});

			return;
		} catch (err) {
			console.error("❌ Database connectie mislukt:", err.message);
			retries -= 1;
			console.log("⏳ Wachten voor volgende poging...");
			await sleep(5000);
		}
	}

	console.error(
		" Kon geen verbinding maken met database na 5 pogingen. Server stopt."
	);
	process.exit(1);
};

startServer();

app.post("/api/log", async (req, res) => {
	const { uid, type, data } = req.body;
	try {
		await pool.query(
			`INSERT INTO users (uid, platform) VALUES ($1, 'Unity') ON CONFLICT (uid) DO NOTHING`,
			[uid]
		);
		await pool.query(
			`INSERT INTO game_actions (user_uid, action_type, action_data) VALUES ($1, $2, $3)`,
			[uid, type, data]
		);
		console.log(` Data opgeslagen voor user: ${uid}`);
		res.json({ status: "success" });
	} catch (err) {
		console.error(err);
		res.status(500).json({ error: "Save failed" });
	}
});

app.get("/", (req, res) => {
	res.send("Backend is Online.");
});
