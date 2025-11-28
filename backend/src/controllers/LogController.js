const LogService = require("../services/LogService");

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
			
			const result = await LogService.logGameAction(uid, type, data);
			console.log(` Data opgeslagen voor user: ${uid}`);
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
}

module.exports = new LogController();