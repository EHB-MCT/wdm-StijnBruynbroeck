const LogService = require("../services/LogService");

class LogController {
	async logAction(req, res) {
		const { uid, type, data } = req.body;
		
		try {
			const result = await LogService.logGameAction(uid, type, data);
			console.log(` Data opgeslagen voor user: ${uid}`);
			res.json(result);
		} catch (err) {
			console.error(err);
			res.status(500).json({ error: "Save failed" });
		}
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
}

module.exports = new LogController();