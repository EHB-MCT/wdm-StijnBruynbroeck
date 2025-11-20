require("dotenv").config();
const express = require("express");
const cors = require("cors");
const { Pool } = require("pg");

const app = express();
const port = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

const pool = new Pool({
	connectionString: process.env.DATABASE_URL,
});

app.get("/", async (req, res) => {
	try {
		const result = await pool.query("SELECT NOW()");
		res.json({
			message: "Weapon of Math Destruction Backend Online",
			time: result.rows[0].now,
		});
	} catch (err) {
		console.error(err);
		res.status(500).json({ error: "Database connection failed" });
	}
});

app.listen(port, () => {
	console.log(`Server listening on port ${port}`);
});
