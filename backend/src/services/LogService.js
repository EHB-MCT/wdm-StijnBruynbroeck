const pool = require("../config/db");

class LogService {
	async initializeTables() {
		await pool.query(`
                CREATE TABLE IF NOT EXISTS users (
                    user_uid VARCHAR(255) PRIMARY KEY,
                    platform VARCHAR(50),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    last_session TIMESTAMP,
                    total_sessions INTEGER DEFAULT 1,
                    total_playtime FLOAT DEFAULT 0
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

		await pool.query(`
                CREATE TABLE IF NOT EXISTS user_profiles (
                    user_uid VARCHAR(255) PRIMARY KEY,
                    risk_tolerance FLOAT DEFAULT 0.5,
                    decission_speed FLOAT DEFAULT 0.5,
                    resource_efficiency FLOAT DEFAULT 0.5,
                    strategic_score FLOAT DEFAULT 0.5,
                    engagement_level FLOAT DEFAULT 0.5,
                    emotional_responsiveness FLOAT DEFAULT 0.5,
                    influence_susceptibility FLOAT DEFAULT 0.5,
                    skill_progression FLOAT DEFAULT 0.5,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_uid) REFERENCES users(uid) ON DELETE CASCADE
                );
            `);

		await pool.query(`
                CREATE TABLE IF NOT EXISTS influence_events (
                    id SERIAL PRIMARY KEY,
                    user_uid VARCHAR(255),
                    influence_type VARCHAR(50),
                    influence_strength FLOAT,
                    player_response VARCHAR(50),
                    effectiveness_score FLOAT,
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_uid) REFERENCES users(uid) ON DELETE CASCADE
                );
            `);

		await pool.query(`
                CREATE TABLE IF NOT EXISTS behavioral_metrics (
                    id SERIAL PRIMARY KEY,
                    user_uid VARCHAR(255),
                    metric_type VARCHAR(50),
                    metric_value FLOAT,
                    context_data JSONB,
                    session_id VARCHAR(50),
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_uid) REFERENCES users(uid) ON DELETE CASCADE
                );
            `);

		await pool.query(`
                CREATE TABLE IF NOT EXISTS sessions (
                    session_id VARCHAR(50) PRIMARY KEY,
                    user_uid VARCHAR(255),
                    start_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    end_time TIMESTAMP,
                    duration FLOAT,
                    actions_count INTEGER DEFAULT 0,
                    decisions_made INTEGER DEFAULT 0,
                    resources_gained INTEGER DEFAULT 0,
                    resources_spent INTEGER DEFAULT 0,
                    FOREIGN KEY (user_uid) REFERENCES users(uid) ON DELETE CASCADE
                );
            `);
	}

	async logGameAction(uid, type, data) {
		try {
			const dataString = JSON.stringify(data);
			const result = await pool.query(
				`INSERT INTO game_actions (user_uid, action_type, action_data) VALUES ($1, $2, $3)`,
				[uid, type, dataString]
			);
			return { status: "success" };
		} catch (error) {
			console.error('SQL Error in logGameAction:', error);
			throw error;
		}
	}
		
		await pool.query(
			`INSERT INTO $1, user_uid, action_type, action_data) VALUES ($1, $2, $3)`,
			[uid, type, data]
		);
		
		return { status: "success" };
	}

	async getGameActions(uid) {
		const result = await pool.query(
			`SELECT * FROM game_actions WHERE $1, user_uid = $1 ORDER BY timestamp DESC`,
			[uid]
		);
		return result.rows;
	}

	async updateUserProfile(uid, profileData) {
		await pool.query(`
			INSERT INTO $1, user_uid, risk_tolerance, decision_speed, resource_efficiency, strategic_score, engagement_level, emotional_responsiveness, influence_susceptibility, skill_progression)
			VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
			ON CONFLICT (uid) DO UPDATE SET
				risk_tolerance = EXCLUDED.risk_tolerance,
				decision_speed = EXCLUDED.decision_speed,
				resource_efficiency = EXCLUDED.resource_efficiency,
				strategic_score = EXCLUDED.strategic_score,
				engagement_level = EXCLUDED.engagement_level,
				emotional_responsiveness = EXCLUDED.emotional_responsiveness,
				influence_susceptibility = EXCLUDED.influence_susceptibility,
				skill_progression = EXCLUDED.skill_progression,
				updated_at = CURRENT_TIMESTAMP
		`, [uid, profileData.risk_tolerance, profileData.decision_speed, profileData.resource_efficiency, 
			profileData.strategic_score, profileData.engagement_level, profileData.emotional_responsiveness,
			profileData.influence_susceptibility, profileData.skill_progression]);
	}

	async getUserProfile(uid) {
		const result = await pool.query(
			`SELECT * FROM user_profiles WHERE $1, user_uid = $1`,
			[uid]
		);
		return result.rows[0] || null;
	}

	async analyzeBehavioralPatterns(uid) {
		// Get recent actions for pattern analysis
		const actionsResult = await pool.query(
			`SELECT action_type, action_data, timestamp FROM game_actions 
			 WHERE $1, user_uid = $1 AND timestamp > NOW() - INTERVAL '24 hours'
			 ORDER BY timestamp DESC`,
			[uid]
		);

		const actions = actionsResult.rows;
		const analysis = this.calculateBehavioralMetrics(actions);
		
		// Update user profile with new analysis
		await this.updateUserProfile(uid, analysis);
		
		return analysis;
	}

	calculateBehavioralMetrics(actions) {
		const metrics = {
			risk_tolerance: 0.5,
			decision_speed: 0.5,
			resource_efficiency: 0.5,
			strategic_score: 0.5,
			engagement_level: 0.5,
			emotional_responsiveness: 0.5,
			influence_susceptibility: 0.5,
			skill_progression: 0.5
		};

		// Analyze decision timing
		const decisionTimings = actions.filter(a => a.action_type === 'DecisionTiming');
		if (decisionTimings.length > 0) {
			const avgTime = decisionTimings.reduce((sum, action) => {
				const time = parseFloat(action.action_data.details.split(':')[1]);
				return sum + time;
			}, 0) / decisionTimings.length;
			
			// Faster decisions = higher decision_speed (but not too fast = reckless)
			metrics.decision_speed = avgTime < 2 ? 0.8 : avgTime < 5 ? 0.6 : avgTime < 10 ? 0.4 : 0.2;
		}

		// Analyze resource management
		const resourceActions = actions.filter(a => a.action_type === 'ResourceManagement');
		if (resourceActions.length > 0) {
			const spentActions = resourceActions.filter(a => a.action_data.details.includes('Spent'));
			const foundActions = resourceActions.filter(a => a.action_data.details.includes('Found'));
			
			// Good resource management = more found than spent
			metrics.resource_efficiency = foundActions.length > spentActions.length ? 0.8 : 
										   foundActions.length === spentActions.length ? 0.5 : 0.3;
		}

		// Analyze threat responses for risk tolerance
		const threatResponses = actions.filter(a => a.action_type === 'ThreatResponse');
		if (threatResponses.length > 0) {
			const paidOffCount = threatResponses.filter(a => a.action_data.details.includes('true')).length;
			metrics.risk_tolerance = paidOffCount / threatResponses.length;
		}

		// Calculate engagement based on action frequency
		metrics.engagement_level = Math.min(actions.length / 50, 1.0);

		// Analyze emotional responses
		const emotionalResponses = actions.filter(a => a.action_type === 'EmotionalResponse');
		if (emotionalResponses.length > 0) {
			const positiveResponses = emotionalResponses.filter(a => a.action_data.details.includes('Positive')).length;
			metrics.emotional_responsiveness = positiveResponses / emotionalResponses.length;
		}

		return metrics;
	}

	async getAllUsers() {
		const result = await pool.query(`
			SELECT u.uid, u.created_at, u.last_session, u.total_sessions, u.total_playtime
			FROM users u
			ORDER BY u.created_at DESC
		`);
		return result.rows;
	}

	async getUserActionsByType(uid, actionType, limit = 100) {
		const result = await pool.query(
			`INSERT INTO game_actions (user_uid, action_type, action_data) VALUES ($1, $2, $3)`,
			[uid, type, data]
		);
		return result.rows;
	}

	async getUserSessions(uid) {
		const result = await pool.query(
			`SELECT * FROM sessions WHERE $1, user_uid = $1 ORDER BY start_time DESC`,
			[uid]
		);
		return result.rows;
	}

	async recordInfluenceEvent(uid, influenceType, influenceStrength, playerResponse, effectivenessScore) {
		await pool.query(
			`INSERT INTO $1, user_uid, influence_type, influence_strength, player_response, effectiveness_score)
			 VALUES ($1, $2, $3, $4, $5)`,
			[uid, influenceType, influenceStrength, playerResponse, effectivenessScore]
		);
	}
}

module.exports = new LogService();