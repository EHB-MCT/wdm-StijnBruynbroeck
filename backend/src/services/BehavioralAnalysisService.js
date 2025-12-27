const LogService = require('./LogService');
const pool = require('../config/db');

class BehavioralAnalysisService {
	async updateUserProfileRealtime(uid) {
		try {
			// Get recent actions for pattern analysis
			const recentActions = await LogService.getGameActions(uid);
			
			// Calculate comprehensive behavioral metrics
			const analysis = await this.performBehavioralAnalysis(uid, recentActions);
			
			// Update user profile with new metrics
			await LogService.updateUserProfile(uid, analysis);
			
			// Store behavioral metrics aggregation
			await this.storeBehavioralMetrics(uid, analysis);
			
			return analysis;
		} catch (error) {
			console.error(`Error updating user profile for ${uid}:`, error);
			throw error;
		}
	}

	async storeBehavioralMetrics(uid, analysis) {
		try {
			const now = new Date();
			const oneDayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);
			
			// Store individual metrics for detailed analysis
			const metrics = [
				['risk_tolerance_score', analysis.risk_tolerance, {source: 'threat_analysis'}],
				['adaptability_index', analysis.decision_speed, {source: 'timing_analysis'}],
				['engagement_score', analysis.engagement_level, {source: 'activity_analysis'}],
				['analytical_vs_intuitive', analysis.strategic_score, {source: 'strategy_analysis'}],
				['influence_susceptibility', analysis.influence_susceptibility, {source: 'influence_analysis'}],
				['skill_level', analysis.skill_progression, {source: 'progression_analysis'}]
			];

			for (const [metricType, value, context] of metrics) {
				await pool.query(
					`INSERT INTO behavioral_metrics 
					 (uid, metric_type, metric_value, metric_context, aggregation_period, aggregation_start, aggregation_end, sample_size)
					 VALUES ($1, $2, $3, $4, 'session', $5, $6, 1)
					 ON CONFLICT (uid, metric_type, aggregation_period, aggregation_start) 
					 DO UPDATE SET metric_value = EXCLUDED.metric_value, metric_context = EXCLUDED.metric_context`,
					[uid, metricType, value, JSON.stringify(context), oneDayAgo, now]
				);
			}

			// Update profile confidence based on data points analyzed
			await this.updateProfileConfidence(uid, analysis.dataPointsCount || 10);
			
		} catch (error) {
			console.error('Error storing behavioral metrics:', error);
		}
	}

	async updateProfileConfidence(uid, dataPointsCount) {
		try {
			const confidence = 1.0 - Math.exp(-dataPointsCount / 10.0); // Logistic function
			
			await pool.query(
				`UPDATE user_profiles 
				 SET profile_confidence = $1, data_points_analyzed = $2
				 WHERE uid = $3`,
				[confidence, dataPointsCount, uid]
			);
		} catch (error) {
			console.error('Error updating profile confidence:', error);
		}
	}

	async createOrUpdateUserProfile(uid, profileData) {
		try {
			const existingProfile = await pool.query(
				'SELECT id FROM user_profiles WHERE uid = $1 ORDER BY profile_date DESC LIMIT 1',
				[uid]
			);

			if (existingProfile.rows.length === 0) {
				// Create new profile
				await pool.query(
					`INSERT INTO user_profiles 
					 (uid, player_type, risk_tolerance_score, adaptability_index, engagement_score,
					  decision_speed_avg, analytical_vs_intuitive, influence_susceptibility, skill_level,
					  profile_confidence, data_points_analyzed)
					 VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)`,
					[
						uid,
						this.determinePlayerType(profileData),
						profileData.risk_tolerance || 0.5,
						profileData.decision_speed || 0.5,
						profileData.engagement_level || 0.5,
						profileData.decision_speed || 0.5,
						profileData.strategic_score || 0.5,
						profileData.influence_susceptibility || 0.5,
						profileData.skill_progression || 0.5,
						profileData.confidence || 0.5,
						profileData.dataPointsCount || 10
					]
				);
			} else {
				// Update existing profile
				await pool.query(
					`UPDATE user_profiles 
					 SET player_type = $1, risk_tolerance_score = $2, adaptability_index = $3, 
					     engagement_score = $4, decision_speed_avg = $5, analytical_vs_intuitive = $6,
					     influence_susceptibility = $7, skill_level = $8, profile_confidence = $9,
					     data_points_analyzed = $10, profile_date = CURRENT_TIMESTAMP
					 WHERE uid = $11`,
					[
						this.determinePlayerType(profileData),
						profileData.risk_tolerance || 0.5,
						profileData.decision_speed || 0.5,
						profileData.engagement_level || 0.5,
						profileData.decision_speed || 0.5,
						profileData.strategic_score || 0.5,
						profileData.influence_susceptibility || 0.5,
						profileData.skill_progression || 0.5,
						profileData.confidence || 0.5,
						profileData.dataPointsCount || 10,
						uid
					]
				);
			}
		} catch (error) {
			console.error('Error creating/updating user profile:', error);
			throw error;
		}
	}

	async getUserAnalytics(uid) {
		try {
			const result = await pool.query(
				'SELECT * FROM user_analytics_view WHERE uid = $1',
				[uid]
			);
			
			return result.rows[0] || null;
		} catch (error) {
			console.error('Error getting user analytics:', error);
			return null;
		}
	}

	async detectBehavioralPatterns(uid) {
		try {
			const actions = await LogService.getGameActions(uid);
			const patterns = {
				time_of_day_patterns: await this.analyzeTimeOfDayPatterns(actions),
				session_length_patterns: await this.analyzeSessionLengthPatterns(uid),
				progression_patterns: await this.analyzeProgressionPatterns(actions),
				influence_response_patterns: await this.analyzeInfluenceResponsePatterns(uid, actions),
				emotional_state_patterns: await this.analyzeEmotionalStatePatterns(actions)
			};
			
			return patterns;
		} catch (error) {
			console.error('Error detecting behavioral patterns:', error);
			return null;
		}
	}

	async analyzeTimeOfDayPatterns(actions) {
		const hourlyActivity = new Array(24).fill(0);
		
		actions.forEach(action => {
			const hour = new Date(action.created_at).getHours();
			hourlyActivity[hour]++;
		});
		
		const peakHours = hourlyActivity
			.map((count, hour) => ({ hour, count }))
			.sort((a, b) => b.count - a.count)
			.slice(0, 3)
			.map(item => item.hour);
		
		return {
			hourly_distribution: hourlyActivity,
			peak_hours: peakHours,
			most_active_period: this.determineActivePeriod(peakHours)
		};
	}

	async analyzeSessionLengthPatterns(uid) {
		const result = await pool.query(
			'SELECT session_duration FROM user_sessions WHERE uid = $1 AND session_end IS NOT NULL',
			[uid]
		);
		
		const sessions = result.rows.map(row => row.session_duration).filter(d => d > 0);
		
		if (sessions.length === 0) return null;
		
		const avgSessionLength = sessions.reduce((sum, duration) => sum + duration, 0) / sessions.length;
		const shortestSession = Math.min(...sessions);
		const longestSession = Math.max(...sessions);
		
		return {
			average_session_length: avgSessionLength,
			shortest_session: shortestSession,
			longest_session: longestSession,
			total_sessions: sessions.length,
			session_consistency: 1 - (longestSession - shortestSession) / avgSessionLength
		};
	}

	async analyzeProgressionPatterns(actions) {
		const progressionActions = actions.filter(a => 
			['StrategicChoice', 'BuildSuccess', 'QuestDecision'].includes(a.action_type)
		);
		
		if (progressionActions.length < 5) return null;
		
		// Group actions by time periods (e.g., every 10 minutes)
		const timeWindows = {};
		progressionActions.forEach(action => {
			const timeWindow = Math.floor(action.time_in_game / 600); // 10-minute windows
			if (!timeWindows[timeWindow]) timeWindows[timeWindow] = [];
			timeWindows[timeWindow].push(action);
		});
		
		const progressionCurve = Object.keys(timeWindows)
			.sort((a, b) => a - b)
			.map(window => ({
				time_window: parseInt(window) * 10,
				success_rate: this.calculateSuccessRate(timeWindows[window]),
				action_count: timeWindows[window].length
			}));
		
		return {
			learning_curve: progressionCurve,
			improvement_rate: this.calculateImprovementRate(progressionCurve),
			mastery_time: this.estimateMasteryTime(progressionCurve)
		};
	}

	async analyzeInfluenceResponsePatterns(uid, actions) {
		const result = await pool.query(
			'SELECT influence_type, influence_effectiveness, player_resistance FROM influence_events WHERE uid = $1',
			[uid]
		);
		
		if (result.rows.length === 0) return null;
		
		const influenceResponses = {};
		result.rows.forEach(row => {
			if (!influenceResponses[row.influence_type]) {
				influenceResponses[row.influence_type] = {
					exposures: 0,
					effectiveness_sum: 0,
					resistance_count: 0
				};
			}
			influenceResponses[row.influence_type].exposures++;
			influenceResponses[row.influence_type].effectiveness_sum += row.influence_effectiveness;
			if (row.player_resistance) influenceResponses[row.influence_type].resistance_count++;
		});
		
		const patterns = {};
		Object.keys(influenceResponses).forEach(influenceType => {
			const data = influenceResponses[influenceType];
			patterns[influenceType] = {
				exposures: data.exposures,
				average_effectiveness: data.effectiveness_sum / data.exposures,
				resistance_rate: data.resistance_count / data.exposures,
				susceptibility: 1 - (data.resistance_count / data.exposures)
			};
		});
		
		return patterns;
	}

	async analyzeEmotionalStatePatterns(actions) {
		const emotionalActions = actions.filter(a => a.action_type === 'EmotionalResponse');
		
		if (emotionalActions.length === 0) return null;
		
		const emotionalStates = {};
		emotionalActions.forEach(action => {
			const parts = action.action_data.details.split(':');
			const emotion = parts[1] || 'neutral';
			const responseSpeed = parseFloat(parts[2]) || 0;
			
			if (!emotionalStates[emotion]) {
				emotionalStates[emotion] = { count: 0, totalResponseSpeed: 0 };
			}
			emotionalStates[emotion].count++;
			emotionalStates[emotion].totalResponseSpeed += responseSpeed;
		});
		
		const patterns = {};
		Object.keys(emotionalStates).forEach(emotion => {
			const data = emotionalStates[emotion];
			patterns[emotion] = {
				frequency: data.count,
				average_response_speed: data.totalResponseSpeed / data.count,
				percentage: (data.count / emotionalActions.length) * 100
			};
		});
		
		return {
			emotional_distribution: patterns,
			dominant_emotion: Object.keys(patterns).reduce((a, b) => 
				patterns[a].frequency > patterns[b].frequency ? a : b
			),
			emotional_volatility: this.calculateEmotionalVolatility(patterns)
		};
	}

	determineActivePeriod(peakHours) {
		if (peakHours.every(hour => hour >= 6 && hour < 18)) return 'Day Active';
		if (peakHours.every(hour => hour >= 18 || hour < 6)) return 'Night Active';
		return 'Mixed Pattern';
	}

	calculateImprovementRate(progressionCurve) {
		if (progressionCurve.length < 2) return 0;
		
		const firstRate = progressionCurve[0].success_rate;
		const lastRate = progressionCurve[progressionCurve.length - 1].success_rate;
		
		return (lastRate - firstRate) / progressionCurve.length;
	}

	estimateMasteryTime(progressionCurve) {
		const threshold = 0.8; // 80% success rate = mastery
		for (const point of progressionCurve) {
			if (point.success_rate >= threshold) {
				return point.time_window;
			}
		}
		return null; // Not yet reached mastery
	}

	calculateEmotionalVolatility(patterns) {
		const emotions = Object.keys(patterns);
		if (emotions.length < 2) return 0;
		
		const percentages = emotions.map(e => patterns[e].percentage);
		const mean = percentages.reduce((sum, p) => sum + p, 0) / percentages.length;
		const variance = percentages.reduce((sum, p) => sum + Math.pow(p - mean, 2), 0) / percentages.length;
		
		return Math.sqrt(variance);
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

	// Enhanced methods for influence system integration
	async getInfluenceRecommendations(uid) {
		try {
			const profile = await this.getUserAnalytics(uid);
			const patterns = await this.detectBehavioralPatterns(uid);
			
			const recommendations = {
				framing_strategies: [],
				anchoring_points: [],
				scarcity_tactics: [],
				social_proof_elements: []
			};

			// Analyze risk tolerance for framing strategies
			if (profile.risk_tolerance_score > 0.6) {
				recommendations.framing_strategies.push({
					type: 'gain_framing',
					description: 'Emphasize potential gains and rewards',
					effectiveness: 0.8
				});
			} else {
				recommendations.framing_strategies.push({
					type: 'loss_aversion',
					description: 'Highlight potential losses and risks',
					effectiveness: 0.7
				});
			}

			// Analyze decision speed for anchoring
			if (profile.decision_speed_avg < 3) {
				recommendations.anchoring_points.push({
					type: 'default_options',
					description: 'Pre-select optimal options for quick decision makers',
					effectiveness: 0.9
				});
			}

			// Analyze emotional patterns for scarcity
			if (patterns && patterns.emotional_state_patterns) {
				const emotionalData = patterns.emotional_state_patterns;
				if (emotionalData.emotional_volatility > 30) {
					recommendations.scarcity_tactics.push({
						type: 'limited_time_offers',
						description: 'Use time-limited opportunities for emotionally responsive players',
						effectiveness: 0.75
					});
				}
			}

			// Analyze engagement for social proof
			if (profile.engagement_score > 0.6) {
				recommendations.social_proof_elements.push({
					type: 'player_statistics',
					description: 'Show what similar players have accomplished',
					effectiveness: 0.8
				});
			}

			return recommendations;
		} catch (error) {
			console.error('Error getting influence recommendations:', error);
			return null;
		}
	}

	async calculateDynamicDifficulty(uid) {
		try {
			const profile = await this.getUserAnalytics(uid);
			const patterns = await this.detectBehavioralPatterns(uid);
			
			let difficultyMultiplier = 1.0;

			// Adjust based on skill level
			if (profile.skill_level > 0.7) difficultyMultiplier += 0.2;
			else if (profile.skill_level < 0.3) difficultyMultiplier -= 0.2;

			// Adjust based on recent performance
			if (patterns && patterns.progression_patterns) {
				const improvementRate = patterns.progression_patterns.improvement_rate;
				if (improvementRate > 0.1) difficultyMultiplier += 0.1;
				else if (improvementRate < -0.1) difficultyMultiplier -= 0.1;
			}

			// Adjust based on frustration levels (from emotional patterns)
			if (patterns && patterns.emotional_state_patterns) {
				const emotionalData = patterns.emotional_state_patterns;
				if (emotionalData.dominant_emotion === 'frustrated') {
					difficultyMultiplier -= 0.15;
				}
			}

			return Math.max(0.5, Math.min(2.0, difficultyMultiplier));
		} catch (error) {
			console.error('Error calculating dynamic difficulty:', error);
			return 1.0;
		}
	}

	async getABTestAssignment(uid, testName) {
		try {
			// Check if user is already assigned to this test
			const existingAssignment = await pool.query(
				'SELECT assignment_group FROM user_ab_assignments uaa JOIN ab_tests ab ON uaa.test_id = ab.id WHERE uaa.uid = $1 AND ab.test_name = $2',
				[uid, testName]
			);

			if (existingAssignment.rows.length > 0) {
				return existingAssignment.rows[0].assignment_group;
			}

			// Get test configuration
			const testResult = await pool.query(
				'SELECT id, traffic_split FROM ab_tests WHERE test_name = $1 AND is_active = true',
				[testName]
			);

			if (testResult.rows.length === 0) {
				return 'control'; // Default to control if test not found
			}

			const test = testResult.rows[0];
			const random = Math.random();
			const assignmentGroup = random < test.traffic_split ? 'control' : 'variant';

			// Assign user to test
			await pool.query(
				'INSERT INTO user_ab_assignments (uid, test_id, assignment_group) VALUES ($1, $2, $3)',
				[uid, test.id, assignmentGroup]
			);

			return assignmentGroup;
		} catch (error) {
			console.error('Error getting A/B test assignment:', error);
			return 'control';
		}
	}

	async recordInfluenceEvent(uid, influenceData) {
		try {
			await pool.query(
				`INSERT INTO influence_events 
				 (uid, influence_type, influence_mechanism, influence_parameters, game_context, 
				  player_profile_snapshot, player_action, influence_effectiveness, player_resistance)
				 VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)`,
				[
					uid,
					influenceData.type,
					influenceData.mechanism,
					JSON.stringify(influenceData.parameters),
					influenceData.context,
					JSON.stringify(influenceData.profileSnapshot),
					influenceData.playerAction,
					influenceData.effectiveness || 0,
					influenceData.playerResisted || false
				]
			);
		} catch (error) {
			console.error('Error recording influence event:', error);
		}
	}

	async analyzeInfluenceEffectiveness(testName) {
		try {
			const result = await pool.query(
				`SELECT 
				  ab.test_name,
				  uaa.assignment_group,
				  COUNT(*) as participants,
				  COUNT(uaa.converted) as conversions,
				  AVG(ie.influence_effectiveness) as avg_effectiveness
				 FROM ab_tests ab
				 JOIN user_ab_assignments uaa ON ab.id = uaa.test_id
				 LEFT JOIN influence_events ie ON uaa.uid = ie.uid AND ab.test_name = $1
				 WHERE ab.test_name = $1
				 GROUP BY ab.test_name, uaa.assignment_group`,
				[testName]
			);

			return result.rows;
		} catch (error) {
			console.error('Error analyzing influence effectiveness:', error);
			return [];
		}
	}
}

module.exports = new BehavioralAnalysisService();