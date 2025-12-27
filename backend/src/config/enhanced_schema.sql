-- Enhanced Database Schema for User Profiling and Behavioral Analysis
-- Weapon of Math Destruction - Complete Behavioral Profiling System

-- Users table with enhanced profiling data
CREATE TABLE IF NOT EXISTS users (
    uid VARCHAR(50) PRIMARY KEY,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_active TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    total_sessions INTEGER DEFAULT 0,
    total_playtime FLOAT DEFAULT 0,
    demographics JSONB DEFAULT '{}',
    preferences JSONB DEFAULT '{}',
    device_info JSONB DEFAULT '{}'
);

-- User behavioral profiles
CREATE TABLE IF NOT EXISTS user_profiles (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    profile_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Strategic player characteristics
    player_type VARCHAR(50), -- 'Aggressive', 'Defensive', 'Balanced', 'Random'
    risk_tolerance_score FLOAT DEFAULT 0.0, -- -1.0 (risk-averse) to 1.0 (risk-seeking)
    adaptability_index FLOAT DEFAULT 0.0, -- 0.0 (slow learner) to 1.0 (fast adapter)
    engagement_score FLOAT DEFAULT 0.0, -- 0.0 (disengaged) to 1.0 (highly engaged)
    
    -- Decision-making patterns
    decision_speed_avg FLOAT DEFAULT 0.0, -- Average decision time in seconds
    analytical_vs_intuitive FLOAT DEFAULT 0.0, -- -1.0 (intuitive) to 1.0 (analytical)
    stress_response FLOAT DEFAULT 0.0, -- Performance degradation under pressure
    
    -- Influence susceptibility
    influence_susceptibility FLOAT DEFAULT 0.0, -- 0.0 (resistant) to 1.0 (highly susceptible)
    framing_sensitivity FLOAT DEFAULT 0.0, -- Response to framing effects
    anchoring_bias_score FLOAT DEFAULT 0.0, -- Tendency to use anchors
    scarcity_response FLOAT DEFAULT 0.0, -- Response to scarcity/urgency
    
    -- Skill and progression
    skill_level FLOAT DEFAULT 0.0, -- Overall game skill assessment
    learning_rate FLOAT DEFAULT 0.0, -- How quickly player improves
    mastery_areas JSONB DEFAULT '[]', -- Areas where player excels
    
    profile_confidence FLOAT DEFAULT 0.0, -- Confidence in profile accuracy (0-1)
    data_points_analyzed INTEGER DEFAULT 0
);

-- Enhanced user interactions tracking
CREATE TABLE IF NOT EXISTS user_interactions (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    interaction_type VARCHAR(100) NOT NULL,
    interaction_data JSONB NOT NULL,
    session_id VARCHAR(50),
    
    -- Spatial and temporal context
    time_in_game FLOAT NOT NULL,
    hex_coordinate_x INTEGER DEFAULT -1,
    hex_coordinate_y INTEGER DEFAULT -1,
    
    -- Behavioral context
    decision_time FLOAT DEFAULT NULL, -- Time from trigger to action
    emotional_state VARCHAR(50) DEFAULT NULL, -- 'frustrated', 'excited', 'neutral', etc.
    pressure_level FLOAT DEFAULT 0.0, -- Current game pressure (0-1)
    
    -- Influence context
    influence_type VARCHAR(50) DEFAULT NULL, -- Type of influence applied
    influence_strength FLOAT DEFAULT 0.0, -- Strength of influence (0-1)
    was_influenced BOOLEAN DEFAULT FALSE, -- Whether player was influenced
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Indexes for performance
    INDEX idx_user_interactions_uid (uid),
    INDEX idx_user_interactions_type (interaction_type),
    INDEX idx_user_interactions_created (created_at),
    INDEX idx_user_interactions_session (session_id)
);

-- Influence events tracking
CREATE TABLE IF NOT EXISTS influence_events (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    influence_type VARCHAR(50) NOT NULL, -- 'framing', 'anchoring', 'scarcity', 'social_proof', etc.
    influence_mechanism VARCHAR(100) NOT NULL, -- Specific mechanism used
    influence_parameters JSONB NOT NULL, -- Parameters for the influence
    
    -- Context
    game_context VARCHAR(100) NOT NULL, -- Where/when influence was applied
    player_profile_snapshot JSONB, -- Player profile at time of influence
    
    -- Outcome
    player_action VARCHAR(100) NOT NULL, -- What player actually did
    influence_effectiveness FLOAT DEFAULT 0.0, -- Measured effectiveness (0-1)
    player_resistance BOOLEAN DEFAULT FALSE, -- Whether player resisted influence
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_influence_events_uid (uid),
    INDEX idx_influence_events_type (influence_type),
    INDEX idx_influence_events_created (created_at)
);

-- Behavioral metrics aggregation
CREATE TABLE IF NOT EXISTS behavioral_metrics (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    metric_type VARCHAR(100) NOT NULL, -- 'decision_timing', 'risk_taking', 'engagement', etc.
    metric_value FLOAT NOT NULL,
    metric_context JSONB DEFAULT '{}', -- Additional context for the metric
    
    -- Aggregation info
    aggregation_period VARCHAR(20), -- 'session', 'day', 'week', 'month'
    aggregation_start TIMESTAMP NOT NULL,
    aggregation_end TIMESTAMP NOT NULL,
    sample_size INTEGER DEFAULT 1, -- Number of data points aggregated
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_behavioral_metrics_uid (uid),
    INDEX idx_behavioral_metrics_type (metric_type),
    INDEX idx_behavioral_metrics_period (aggregation_period)
);

-- Session tracking
CREATE TABLE IF NOT EXISTS user_sessions (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    session_id VARCHAR(50) UNIQUE NOT NULL,
    
    session_start TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    session_end TIMESTAMP DEFAULT NULL,
    session_duration FLOAT DEFAULT NULL, -- Duration in seconds
    
    -- Session statistics
    total_interactions INTEGER DEFAULT 0,
    decisions_made INTEGER DEFAULT 0,
    threats_faced INTEGER DEFAULT 0,
    quests_completed INTEGER DEFAULT 0,
    resources_collected INTEGER DEFAULT 0,
    
    -- Performance metrics
    final_score INTEGER DEFAULT 0,
    achievements_unlocked JSONB DEFAULT '[]',
    errors_made INTEGER DEFAULT 0,
    
    -- Technical metrics
    device_type VARCHAR(50) DEFAULT NULL,
    connection_quality FLOAT DEFAULT NULL,
    fps_average FLOAT DEFAULT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_user_sessions_uid (uid),
    INDEX idx_user_sessions_session (session_id),
    INDEX idx_user_sessions_start (session_start)
);

-- A/B testing framework
CREATE TABLE IF NOT EXISTS ab_tests (
    id SERIAL PRIMARY KEY,
    test_name VARCHAR(100) UNIQUE NOT NULL,
    test_description TEXT,
    
    -- Test configuration
    control_parameters JSONB NOT NULL,
    variant_parameters JSONB NOT NULL,
    traffic_split FLOAT DEFAULT 0.5, -- Percentage for control (0-1)
    
    -- Test status
    is_active BOOLEAN DEFAULT TRUE,
    start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_date TIMESTAMP DEFAULT NULL,
    
    -- Results
    control_participants INTEGER DEFAULT 0,
    variant_participants INTEGER DEFAULT 0,
    control_conversions INTEGER DEFAULT 0,
    variant_conversions INTEGER DEFAULT 0,
    statistical_significance FLOAT DEFAULT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_ab_tests_name (test_name),
    INDEX idx_ab_tests_active (is_active)
);

-- User A/B test assignments
CREATE TABLE IF NOT EXISTS user_ab_assignments (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) REFERENCES users(uid) ON DELETE CASCADE,
    test_id INTEGER REFERENCES ab_tests(id) ON DELETE CASCADE,
    
    assignment_group VARCHAR(10) NOT NULL, -- 'control' or 'variant'
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Results for this user
    converted BOOLEAN DEFAULT FALSE,
    conversion_time TIMESTAMP DEFAULT NULL,
    metrics_collected JSONB DEFAULT '{}',
    
    INDEX idx_user_ab_assignments_uid (uid),
    INDEX idx_user_ab_assignments_test (test_id),
    INDEX idx_user_ab_assignments_group (assignment_group),
    UNIQUE(uid, test_id)
);

-- Data quality and validation
CREATE TABLE IF NOT EXISTS data_quality_metrics (
    id SERIAL PRIMARY KEY,
    metric_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Data completeness
    total_users INTEGER DEFAULT 0,
    active_sessions INTEGER DEFAULT 0,
    missing_uid_count INTEGER DEFAULT 0,
    incomplete_interactions INTEGER DEFAULT 0,
    
    -- Data consistency
    duplicate_sessions INTEGER DEFAULT 0,
    invalid_coordinates INTEGER DEFAULT 0,
    timestamp_anomalies INTEGER DEFAULT 0,
    
    -- System performance
    api_response_time_avg FLOAT DEFAULT 0.0,
    database_query_time_avg FLOAT DEFAULT 0.0,
    error_rate FLOAT DEFAULT 0.0,
    
    INDEX idx_data_quality_metrics_date (metric_date)
);

-- Triggers for automatic profile updates
CREATE OR REPLACE FUNCTION update_user_activity()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE users 
    SET last_active = CURRENT_TIMESTAMP,
        total_sessions = total_sessions + 1
    WHERE uid = NEW.uid;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_user_activity
    AFTER INSERT ON user_sessions
    FOR EACH ROW
    EXECUTE FUNCTION update_user_activity();

-- Function to calculate profile confidence based on data points
CREATE OR REPLACE FUNCTION calculate_profile_confidence(data_points INTEGER)
RETURNS FLOAT AS $$
BEGIN
    -- Logistic function: confidence approaches 1.0 as data points increase
    RETURN 1.0 - EXP(-data_points / 10.0);
END;
$$ LANGUAGE plpgsql;

-- View for comprehensive user analytics
CREATE OR REPLACE VIEW user_analytics_view AS
SELECT 
    u.uid,
    u.created_at,
    u.last_active,
    u.total_sessions,
    u.total_playtime,
    
    -- Latest profile data
    up.player_type,
    up.risk_tolerance_score,
    up.adaptability_index,
    up.engagement_score,
    up.decision_speed_avg,
    up.influence_susceptibility,
    up.skill_level,
    up.profile_confidence,
    
    -- Recent activity
    COUNT(ui.id) as recent_interactions,
    AVG(ui.decision_time) as avg_decision_time,
    MAX(ui.created_at) as last_interaction,
    
    -- Influence response
    COUNT(ie.id) as influence_events,
    AVG(ie.influence_effectiveness) as avg_influence_effectiveness,
    
    -- Session performance
    AVG(us.session_duration) as avg_session_duration,
    SUM(us.final_score) as total_score,
    AVG(us.fps_average) as avg_performance
    
FROM users u
LEFT JOIN user_profiles up ON u.uid = up.uid
LEFT JOIN user_interactions ui ON u.uid = ui.uid AND ui.created_at > CURRENT_DATE - INTERVAL '7 days'
LEFT JOIN influence_events ie ON u.uid = ie.uid
LEFT JOIN user_sessions us ON u.uid = us.uid
GROUP BY u.uid, up.id;