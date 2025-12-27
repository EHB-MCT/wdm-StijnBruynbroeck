const express = require("express");
const LogController = require("../controllers/LogController");

const router = express.Router();

// Original endpoints
router.post("/log", LogController.logAction);
router.get("/actions/:uid", LogController.getActions);

// User profiling endpoints
router.get("/profile/:uid", LogController.getUserProfile);
router.post("/analyze/:uid", LogController.analyzeUserBehavior);
router.get("/users", LogController.getAllUsers);
router.get("/actions/:uid/:actionType", LogController.getUserActionsByType);
router.get("/sessions/:uid", LogController.getUserSessions);
router.post("/influence/:uid", LogController.recordInfluence);

// Behavioral analysis and influence endpoints
router.post("/apply-influence/:uid", LogController.applyInfluence);
router.get("/influence-strategy/:uid", LogController.getInfluenceStrategy);
router.get("/insights/:uid", LogController.getUserInsights);
router.get("/influence-analytics/:uid", LogController.getInfluenceAnalytics);

module.exports = router;