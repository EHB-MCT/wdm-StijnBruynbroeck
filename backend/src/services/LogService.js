const pool = require("../config/db");

class LogService {
	async initializeTables() {
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
	}

	async logGameAction(uid, type, data) {
		await pool.query(
			`INSERT INTO users (uid, platform) VALUES ($1, 'Unity') ON CONFLICT (uid) DO NOTHING`,
			[uid]
		);
		
		await pool.query(
			`INSERT INTO game_actions (user_uid, action_type, action_data) VALUES ($1, $2, $3)`,
			[uid, type, data]
		);
		
		return { status: "success" };
	}

	async getGameActions(uid) {
		const result = await pool.query(
			`SELECT * FROM game_actions WHERE user_uid = $1 ORDER BY timestamp DESC`,
			[uid]
		);
		return result.rows;
	}
}

module.exports = new LogService();