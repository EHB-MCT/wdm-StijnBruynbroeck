const LogService = require('./LogService');

class BehavioralAnalysisService {
	async updateUserProfileRealtime(uid) {
		try {
			// Get recent actions for pattern analysis
			const recentActions = await LogService.getGameActions(uid);
			
			// Calculate comprehensive behavioral metrics
			const analysis = await this.performBehavioralAnalysis(uid, recentActions);
			
			// Update user profile with new metrics
			await LogService.updateUserProfile(uid, analysis);
			
			return analysis;
		} catch (error) {
			console.error(`Error updating user profile for ${uid}:`, error);
			throw error;
		}
	}

	async performBehavioralAnalysis(uid, actions) {
		const profile = {
			risk_tolerance: 0.5,
			decision_speed: 0.5,
			resource_efficiency: 0.5,
			strategic_score: 0.5,
			engagement_level: 0.5,
			emotional_responsiveness: 0.5,
			influence_susceptibility: 0.5,
			skill_progression: 0.5
		};

		// Analyze different aspects of behavior
		profile.risk_tolerance = await this.analyzeRiskTolerance(uid, actions);
		profile.decision_speed = await this.analyzeDecisionSpeed(uid, actions);
		profile.resource_efficiency = await this.analyzeResourceManagement(uid, actions);
		profile.strategic_score = await this.analyzeStrategicThinking(uid, actions);
		profile.engagement_level = await this.analyzeEngagement(uid, actions);
		profile.emotional_responsiveness = await this.analyzeEmotionalResponses(uid, actions);
		profile.skill_progression = await this.analyzeSkillProgression(uid, actions);
		profile.influence_susceptibility = await this.analyzeInfluenceSusceptibility(uid, actions);

		return profile;
	}

	async analyzeRiskTolerance(uid, actions) {
		const threatResponses = actions.filter(a => a.action_type === 'ThreatResponse');
		if (threatResponses.length === 0) return 0.5;

		const paidOffResponses = threatResponses.filter(a => {
			const details = a.action_data.details;
			return details.includes('true'); // Player chose to pay off
		});

		// Higher risk tolerance = more likely to pay off threats
		const riskScore = paidOffResponses.length / threatResponses.length;
		
		// Consider resource levels when analyzing risk tolerance
		const currentResources = await this.getCurrentResources(uid);
		const resourcePenalty = currentResources.total > 20 ? 0.1 : 0;
		
		return Math.max(0, Math.min(1, riskScore - resourcePenalty));
	}

	async analyzeDecisionSpeed(uid, actions) {
		const decisionTimings = actions.filter(a => a.action_type === 'DecisionTiming');
		if (decisionTimings.length === 0) return 0.5;

		const times = decisionTimings.map(action => {
			const details = action.action_data.details;
			return parseFloat(details.split(':')[1]) || 0;
		});

		const avgTime = times.reduce((sum, time) => sum + time, 0) / times.length;
		
		// Optimal decision speed: 2-5 seconds
		if (avgTime < 2) return 0.7; // Very fast - possibly impulsive
		if (avgTime < 5) return 1.0; // Optimal range
		if (avgTime < 10) return 0.6; // Slow but thoughtful
		return 0.3; // Very slow - possibly indecisive
	}

	async analyzeResourceManagement(uid, actions) {
		const resourceActions = actions.filter(a => a.action_type === 'ResourceManagement');
		if (resourceActions.length === 0) return 0.5;

		let totalGained = 0, totalSpent = 0;
		
		resourceActions.forEach(action => {
			const details = action.action_data.details;
			const parts = details.split(':');
			
			if (parts[0] === 'Found') {
				totalGained += parseInt(parts[2]) || 0;
			} else if (parts[0] === 'Spent') {
				totalSpent += parseInt(parts[2]) || 0;
			}
		});

		// Efficiency score based on resource ratio
		const efficiency = totalGained > 0 ? (totalGained / (totalGained + totalSpent)) : 0.5;
		return Math.max(0, Math.min(1, efficiency));
	}

	async analyzeStrategicThinking(uid, actions) {
		const strategicChoices = actions.filter(a => a.action_type === 'StrategicChoice');
		if (strategicChoices.length === 0) return 0.5;

		const successfulChoices = strategicChoices.filter(a => 
			a.action_data.details.includes('BuildSuccess') || 
			a.action_data.details.includes('Victory')
		);

		// Strategic score based on success rate
		const strategicScore = successfulChoices.length / strategicChoices.length;
		
		// Bonus for diverse strategic choices
		const choiceTypes = new Set(strategicChoices.map(a => 
			a.action_data.details.split(':')[0]
		)).size;
		
		const diversityBonus = Math.min(choiceTypes / 5, 0.2);
		return Math.max(0, Math.min(1, strategicScore + diversityBonus));
	}

	async analyzeEngagement(uid, actions) {
		if (actions.length === 0) return 0.5;

		// Engagement based on action frequency and variety
		const actionFrequency = actions.length / 30; // Actions per session (assumed 30 min)
		const actionVariety = new Set(actions.map(a => a.action_type)).size;
		
		// Look for engagement-specific metrics
		const engagementMetrics = actions.filter(a => a.action_type === 'EngagementMetric');
		
		let engagementScore = Math.min(actionFrequency / 10, 0.5);
		engagementScore += Math.min(actionVariety / 10, 0.3);
		engagementScore += Math.min(engagementMetrics.length / 5, 0.2);
		
		return Math.max(0, Math.min(1, engagementScore));
	}

	async analyzeEmotionalResponses(uid, actions) {
		const emotionalResponses = actions.filter(a => a.action_type === 'EmotionalResponse');
		if (emotionalResponses.length === 0) return 0.5;

		const positiveResponses = emotionalResponses.filter(a => 
			a.action_data.details.includes('Positive') || 
			a.action_data.details.includes('Victory')
		);

		const emotionalResponsiveness = positiveResponses.length / emotionalResponses.length;
		
		// Consider response speed (faster responses = higher engagement)
		const avgResponseSpeed = emotionalResponses.reduce((sum, action) => {
			const speed = parseFloat(action.action_data.details.split(':')[2]) || 0;
			return sum + speed;
		}, 0) / emotionalResponses.length;
		
		const speedBonus = avgResponseSpeed < 5 ? 0.1 : 0;
		return Math.max(0, Math.min(1, emotionalResponsiveness + speedBonus));
	}

	async analyzeSkillProgression(uid, actions) {
		// Analyze skill improvement over time
		const recentActions = actions.slice(0, 20); // Last 20 actions
		const olderActions = actions.slice(-20); // Earlier 20 actions

		if (recentActions.length < 10 || olderActions.length < 10) return 0.5;

		const recentSuccessRate = this.calculateSuccessRate(recentActions);
		const olderSuccessRate = this.calculateSuccessRate(olderActions);

		// Progression based on improvement
		const progression = (recentSuccessRate - olderSuccessRate) + 0.5;
		return Math.max(0, Math.min(1, progression));
	}

	async analyzeInfluenceSusceptibility(uid, actions) {
		const influenceEvents = actions.filter(a => a.action_type === 'InfluenceEvent');
		if (influenceEvents.length === 0) return 0.5;

		const influencedActions = influenceEvents.filter(a => 
			a.action_data.details.includes('Accepted') || 
			a.action_data.details.includes('Complied')
		);

		// Base susceptibility on acceptance rate
		const baseSusceptibility = influencedActions.length / influenceEvents.length;
		
		// Consider player personality traits
		const userProfile = await LogService.getUserProfile(uid);
		const personalityModifier = userProfile ? 
			(userProfile.emotional_responsiveness + userProfile.risk_tolerance) / 4 : 0;
		
		return Math.max(0, Math.min(1, baseSusceptibility + personalityModifier));
	}

	calculateSuccessRate(actions) {
		const strategicActions = actions.filter(a => 
			['StrategicChoice', 'BuildSuccess', 'ThreatResponse'].includes(a.action_type)
		);

		if (strategicActions.length === 0) return 0.5;

		const successfulActions = strategicActions.filter(a => 
			a.action_data.details.includes('Success') ||
			a.action_data.details.includes('Built') ||
			a.action_data.details.includes('true')
		);

		return successfulActions.length / strategicActions.length;
	}

	async getCurrentResources(uid) {
		// Get most recent resource data
		const resourceActions = await LogService.getUserActionsByType(uid, 'ResourceManagement', 10);
		
		let gold = 10, wood = 10, food = 15, stone = 5; // Default starting values
		
		// Calculate current resources based on recent actions
		resourceActions.forEach(action => {
			const details = action.action_data.details;
			const parts = details.split(':');
			const actionType = parts[0];
			const resourceType = parts[1];
			const amount = parseInt(parts[2]) || 0;
			const current = parseInt(parts[3]) || 0;
			
			if (resourceType === 'gold') gold = current;
			if (resourceType === 'wood') wood = current;
			if (resourceType === 'food') food = current;
			if (resourceType === 'stone') stone = current;
		});
		
		return {
			gold,
			wood,
			food,
			stone,
			total: gold + wood + food + stone
		};
	}

	async generateUserInsights(uid) {
		const profile = await LogService.getUserProfile(uid);
		const actions = await LogService.getGameActions(uid);
		
		const insights = {
			player_type: this.determinePlayerType(profile),
			strengths: [],
			weaknesses: [],
			recommendations: []
		};

		// Determine player type
		if (profile.risk_tolerance > 0.7) insights.player_type = "Risk-Taker";
		else if (profile.strategic_score > 0.7) insights.player_type = "Strategist";
		else if (profile.resource_efficiency > 0.7) insights.player_type = "Resource Manager";
		else insights.player_type = "Balanced Player";

		// Identify strengths and weaknesses
		if (profile.decision_speed > 0.7) insights.strengths.push("Quick Decision Maker");
		if (profile.strategic_score > 0.7) insights.strengths.push("Strategic Thinker");
		if (profile.resource_efficiency > 0.7) insights.strengths.push("Efficient Resource Manager");
		
		if (profile.decision_speed < 0.3) insights.weaknesses.push("Slow Decision Making");
		if (profile.risk_tolerance < 0.3) insights.weaknesses.push("Risk Averse");
		if (profile.engagement_level < 0.3) insights.weaknesses.push("Low Engagement");

		// Generate recommendations
		if (profile.risk_tolerance < 0.4) {
			insights.recommendations.push("Consider taking calculated risks to improve resource gain");
		}
		if (profile.decision_speed < 0.4) {
			insights.recommendations.push("Practice making decisions more quickly");
		}
		if (profile.strategic_score < 0.5) {
			insights.recommendations.push("Focus on long-term planning over immediate gains");
		}

		return insights;
	}

	determinePlayerType(profile) {
		const traits = [];
		
		if (profile.risk_tolerance > 0.6) traits.push("Aggressive");
		else traits.push("Cautious");
		
		if (profile.decision_speed > 0.6) traits.push("Quick");
		else traits.push("Deliberate");
		
		if (profile.strategic_score > 0.6) traits.push("Strategic");
		else traits.push("Tactical");
		
		return traits.join(" ") + " Player";
	}
}

module.exports = new BehavioralAnalysisService();