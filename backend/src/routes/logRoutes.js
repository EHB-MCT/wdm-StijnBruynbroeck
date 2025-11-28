const express = require("express");
const LogController = require("../controllers/LogController");

const router = express.Router();

router.post("/log", LogController.logAction);
router.get("/actions/:uid", LogController.getActions);

module.exports = router;