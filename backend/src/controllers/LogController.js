const LogService = require("../services/LogService");
const BehavioralAnalysisService = require("../services/BehavioralAnalysisService");
const InfluenceService = require("../services/InfluenceService");

class LogController {
	async logAction(req, res) {
		let body = '';
		
		req.on('data', chunk => {
			body += chunk.toString();
		});
		
	req.on('end', async () => {
		try {
			console.log("Raw body length:", body.length);
			console.log("Raw body:", body);
			console.log("First 10 chars:", body.substring(0, 10));
			
			if (!body || body.trim() === '') {
				throw new Error("Empty body");
			}
			
			const { uid, type, data } = JSON.parse(body);
			
			// Log the action
			const result = await LogService.logGameAction(uid, type, data);
			console.log(` Data opgeslagen voor user: ${uid}`);
			
			// Trigger behavioral analysis for significant events
			if (['DecisionTiming', 'StrategicChoice', 'ThreatResponse', 'QuestDecision'].includes(type)) {
				// Analyze behavior in background (don't wait for response)
				BehavioralAnalysisService.updateUserProfileRealtime(uid).catch(err => {
					console.error('Behavioral analysis failed:', err);
				});
			}
			
			res.json(result);
		} catch (err) {
			console.error("Error:", err);
			res.status(500).json({ error: "Save failed", details: err.message });
		}
	});
	}

	async getActions(req, res) {
		const { uid } = req.params;
		
		try {
			const actions = await LogService.getGameActions(uid);
			res.json(actions);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Fetch failed" });
		}
	}

	async getUserProfile(req, res) {
		const { uid } = req.params;
		
		try {
			const profile = await LogService.getUserProfile(uid);
			res.json(profile);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Profile fetch failed" });
		}
	}

	async analyzeUserBehavior(req, res) {
		const { uid } = req.params;
		
		try {
			const analysis = await LogService.analyzeBehavioralPatterns(uid);
			res.json(analysis);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Analysis failed" });
		}
	}

	async getAllUsers(req, res) {
		try {
			const users = await LogService.getAllUsers();
			res.json(users);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Users fetch failed" });
		}
	}

	async getUserActionsByType(req, res) {
		const { uid, actionType } = req.params;
		const { limit = 100 } = req.query;
		
		try {
			const actions = await LogService.getUserActionsByType(uid, actionType, parseInt(limit));
			res.json(actions);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Actions fetch failed" });
		}
	}

	async getUserSessions(req, res) {
		const { uid } = req.params;
		
		try {
			const sessions = await LogService.getUserSessions(uid);
			res.json(sessions);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Sessions fetch failed" });
		}
	}

	async recordInfluence(req, res) {
		const { uid } = req.params;
		const { influenceType, influenceStrength, playerResponse, effectivenessScore } = req.body;
		
		try {
			await LogService.recordInfluenceEvent(uid, influenceType, influenceStrength, playerResponse, effectivenessScore);
			res.json({ status: "success" });
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Influence recording failed" });
		}
	}

	async applyInfluence(req, res) {
		const { uid } = req.params;
		const { influenceType, context } = req.body;
		
		try {
			const result = await InfluenceService.applyInfluence(uid, influenceType, context);
			res.json(result);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Influence application failed" });
		}
	}

	async getInfluenceStrategy(req, res) {
		const { uid } = req.params;
		const { context } = req.query;
		
		try {
			const strategy = await InfluenceService.generateInfluenceStrategy(uid, context);
			res.json(strategy);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Strategy generation failed" });
		}
	}

	async getUserInsights(req, res) {
		const { uid } = req.params;
		
		try {
			const insights = await BehavioralAnalysisService.generateUserInsights(uid);
			res.json(insights);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Insights generation failed" });
		}
	}

	async getInfluenceAnalytics(req, res) {
		const { uid } = req.params;
		
		try {
			const analytics = await InfluenceService.analyzeInfluenceEffectiveness(uid);
			res.json(analytics);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Analytics generation failed" });
		}
	}
}

module.exports = new LogController();