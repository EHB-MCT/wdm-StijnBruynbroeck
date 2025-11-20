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

app.get("/", (req, res) => {
	res.send("Weapon of Math Destruction Backend is running!");
});

app.listen(port, () => {
	console.log(`Server listening on port ${port}`);
});
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

app.get("/", (req, res) => {
	res.send("Weapon of Math Destruction Backend is running!");
});

app.listen(port, () => {
	console.log(`Server listening on port ${port}`);
});
