const LogService = require('./LogService');
const BehavioralAnalysisService = require('./BehavioralAnalysisService');

class InfluenceService {
	async applyInfluence(uid, influenceType, context) {
		try {
			// Get user profile to determine susceptibility
			const profile = await LogService.getUserProfile(uid);
			if (!profile) {
				// Create default profile if none exists
				await BehavioralAnalysisService.updateUserProfileRealtime(uid);
			}
			
			const susceptibility = profile?.influence_susceptibility || 0.5;
			
			// Apply influence based on type and user susceptibility
			const influenceResult = await this.executeInfluence(uid, influenceType, context, susceptibility);
			
			// Record the influence event for analysis
			await LogService.recordInfluenceEvent(
				uid,
				influenceType,
				influenceResult.strength,
				influenceResult.playerResponse,
				influenceResult.effectiveness
			);
			
			return influenceResult;
		} catch (error) {
			console.error(`Error applying influence for ${uid}:`, error);
			throw error;
		}
	}

	async executeInfluence(uid, influenceType, context, susceptibility) {
		const result = {
			type: influenceType,
			strength: 0.5,
			playerResponse: 'neutral',
			effectiveness: 0,
			modified: false
		};

		switch (influenceType) {
			case 'framing':
				result.strength = 0.6 + (susceptibility * 0.3);
				result = await this.applyFramingEffect(uid, context, result, susceptibility);
				break;
				
			case 'anchoring':
				result.strength = 0.7 + (susceptibility * 0.2);
				result = await this.applyAnchoringEffect(uid, context, result, susceptibility);
				break;
				
			case 'scarcity':
				result.strength = 0.8 + (susceptibility * 0.2);
				result = await this.applyScarcityEffect(uid, context, result, susceptibility);
				break;
				
			case 'social_proof':
				result.strength = 0.5 + (susceptibility * 0.4);
				result = await this.applySocialProofEffect(uid, context, result, susceptibility);
				break;
				
			case 'loss_aversion':
				result.strength = 0.6 + (susceptibility * 0.3);
				result = await this.applyLossAversionEffect(uid, context, result, susceptibility);
				break;
				
			default:
				console.warn(`Unknown influence type: ${influenceType}`);
		}

		return result;
	}

	async applyFramingEffect(uid, context, result, susceptibility) {
		// Frame decisions to emphasize gains vs losses
		const profile = await LogService.getUserProfile(uid);
		
		if (context.includes('threat') || context.includes('payment')) {
			// Frame as opportunity to avoid loss
			if (profile.risk_tolerance < 0.5) {
				// Risk-averse players respond better to loss avoidance
				result.playerResponse = 'accepted';
				result.effectiveness = 0.8;
				result.modified = true;
			}
		} else if (context.includes('building') || context.includes('investment')) {
			// Frame as opportunity for gains
			if (profile.strategic_score > 0.5) {
				result.playerResponse = 'accepted';
				result.effectiveness = 0.7;
				result.modified = true;
			}
		}

		return result;
	}

	async applyAnchoringEffect(uid, context, result, susceptibility) {
		// Use default options as anchors
		const profile = await LogService.getUserProfile(uid);
		
		if (context.includes('resource_cost')) {
			// Anchor with higher prices to make current options seem reasonable
			if (profile.resource_efficiency < 0.6) {
				result.playerResponse = 'accepted';
				result.effectiveness = 0.6;
				result.modified = true;
			}
		}

		return result;
	}

	async applyScarcityEffect(uid, context, result, susceptibility) {
		// Create urgency through scarcity
		const profile = await LogService.getUserProfile(uid);
		
		if (context.includes('opportunity') || context.includes('bonus')) {
			// High urgency for strategic players
			if (profile.strategic_score > 0.5 || profile.engagement_level > 0.6) {
				result.playerResponse = 'accepted';
				result.effectiveness = 0.7;
				result.modified = true;
			}
		}

		return result;
	}

	async applySocialProofEffect(uid, context, result, susceptibility) {
		// Use social proof to influence decisions
		const profile = await LogService.getUserProfile(uid);
		
		// Players with high emotional responsiveness are more susceptible
		if (profile.emotional_responsiveness > 0.6) {
			result.playerResponse = 'accepted';
			result.effectiveness = 0.7;
			result.modified = true;
		}

		return result;
	}

	async applyLossAversionEffect(uid, context, result, susceptibility) {
		// Emphasize potential losses
		const profile = await LogService.getUserProfile(uid);
		
		// Risk-averse players respond strongly to loss aversion
		if (profile.risk_tolerance < 0.4) {
			result.playerResponse = 'accepted';
			result.effectiveness = 0.8;
			result.modified = true;
		}

		return result;
	}

	async generateInfluenceStrategy(uid, currentContext) {
		const profile = await LogService.getUserProfile(uid);
		
		if (!profile) {
			return { strategy: 'none', reason: 'No profile available' };
		}

		const strategy = {
			influenceType: 'none',
			strength: 0,
			message: '',
			modification: null
		};

		// Determine best influence strategy based on user profile
		if (profile.risk_tolerance < 0.4 && currentContext.includes('threat')) {
			// Use loss aversion for risk-averse players
			strategy.influenceType = 'loss_aversion';
			strategy.strength = 0.8;
			strategy.message = "Don't risk losing your progress! Pay the threat now.";
			strategy.modification = { increaseAcceptance: 0.3 };
			
		} else if (profile.strategic_score > 0.6 && currentContext.includes('investment')) {
			// Use social proof for strategic players
			strategy.influenceType = 'social_proof';
			strategy.strength = 0.6;
			strategy.message = "Most successful players make this investment.";
			strategy.modification = { increaseAcceptance: 0.2 };
			
		} else if (profile.resource_efficiency < 0.5 && currentContext.includes('cost')) {
			// Use anchoring for inefficient resource managers
			strategy.influenceType = 'anchoring';
			strategy.strength = 0.7;
			strategy.message = "Special deal! Usually costs much more.";
			strategy.modification = { discountPerception: 0.3 };
			
		} else if (profile.engagement_level > 0.7 && currentContext.includes('opportunity')) {
			// Use scarcity for highly engaged players
			strategy.influenceType = 'scarcity';
			strategy.strength = 0.8;
			strategy.message = "Limited time offer! This opportunity won't last long.";
			strategy.modification = { urgencyMultiplier: 1.5 };
			
		} else {
			// Use general framing
			strategy.influenceType = 'framing';
			strategy.strength = 0.5;
			strategy.message = "Make the smart choice for your empire's future.";
			strategy.modification = { framingBias: 0.2 };
		}

		return strategy;
	}

	async optimizeInfluenceStrength(uid) {
		// Adjust influence strength based on recent responses
		const recentInfluenceEvents = await this.getRecentInfluenceEvents(uid, 10);
		
		if (recentInfluenceEvents.length < 3) {
			return 0.5; // Default strength
		}

		const acceptanceRate = recentInfluenceEvents.filter(event => 
			event.player_response === 'accepted'
		).length / recentInfluenceEvents.length;

		const avgEffectiveness = recentInfluenceEvents.reduce((sum, event) => 
			sum + event.effectiveness_score, 0
		) / recentInfluenceEvents.length;

		// Optimize based on acceptance rate and effectiveness
		if (acceptanceRate > 0.8 && avgEffectiveness > 0.7) {
			return 0.3; // Reduce influence if too effective
		} else if (acceptanceRate < 0.3) {
			return 0.7; // Increase influence if not working
		} else {
			return 0.5; // Maintain current strength
		}
	}

	async getRecentInfluenceEvents(uid, limit) {
		try {
			// This would need to be implemented in LogService
			// For now, return empty array
			return [];
		} catch (error) {
			console.error('Error fetching recent influence events:', error);
			return [];
		}
	}

	async analyzeInfluenceEffectiveness(uid, timeWindow = '24 hours') {
		const profile = await LogService.getUserProfile(uid);
		const actions = await LogService.getGameActions(uid);
		
		const analysis = {
			totalInfluences: 0,
			acceptedInfluences: 0,
			effectivenessScore: 0,
			influenceTypes: {},
			susceptibilityTrend: 'stable'
		};

		// Count influence events and their outcomes
		const influenceActions = actions.filter(a => a.action_type === 'InfluenceEvent');
		analysis.totalInfluences = influenceActions.length;

		influenceActions.forEach(action => {
			const type = action.action_data.details.split(':')[0];
			analysis.influenceTypes[type] = (analysis.influenceTypes[type] || 0) + 1;
		});

		// Calculate effectiveness
		if (analysis.totalInfluences > 0) {
			analysis.effectivenessScore = analysis.acceptedInfluences / analysis.totalInfluences;
		}

		// Determine susceptibility trend
		if (profile) {
			const currentSusceptibility = profile.influence_susceptibility;
			// Compare with historical data if available
			analysis.susceptibilityTrend = currentSusceptibility > 0.6 ? 'increasing' : 
										 currentSusceptibility < 0.4 ? 'decreasing' : 'stable';
		}

		return analysis;
	}
}

module.exports = new InfluenceService();