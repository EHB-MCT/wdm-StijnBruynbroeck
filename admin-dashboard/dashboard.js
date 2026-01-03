// Dashboard JavaScript
class BehavioralDashboard {
    constructor() {
        this.apiBase = 'http://localhost:8080/api';
        this.currentUser = null;
        this.charts = {};
        this.initializeEventListeners();
        this.loadInitialData();
    }

    initializeEventListeners() {
        // User selection
        document.getElementById('userSelect').addEventListener('change', (e) => {
            this.currentUser = e.target.value;
            if (this.currentUser) {
                this.loadUserProfile();
                this.loadUserActions();
                this.generateUserInsights();
            }
        });

        // Buttons
        document.getElementById('refreshBtn').addEventListener('click', () => {
            this.loadInitialData();
        });

        document.getElementById('exportBtn').addEventListener('click', () => {
            this.exportData();
        });

        document.getElementById('analyzeUserBtn').addEventListener('click', () => {
            this.analyzeCurrentUser();
        });

        document.getElementById('generateStrategyBtn').addEventListener('click', () => {
            this.generateInfluenceStrategy();
        });

        document.getElementById('actionTypeFilter').addEventListener('change', (e) => {
            this.filterActions(e.target.value);
        });
    }

    async loadInitialData() {
        this.showLoading(true);
        try {
            await Promise.all([
                this.loadUsers(),
                this.loadOverviewStats()
            ]);
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showToast('Error loading dashboard data', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    async loadUsers() {
        try {
            const response = await axios.get(`${this.apiBase}/users`);
            const users = response.data;
            
            const userSelect = document.getElementById('userSelect');
            userSelect.innerHTML = '<option value="">Select User...</option>';
            
            users.forEach(user => {
                const option = document.createElement('option');
                option.value = user.uid;
                const lastActive = user.last_active ? new Date(user.last_active).toLocaleDateString() : 'Never';
                option.textContent = `${user.uid} (Sessions: ${user.total_sessions || 1}, Last: ${lastActive})`;
                userSelect.appendChild(option);
            });
        } catch (error) {
            console.error('Error loading users:', error);
            throw error;
        }
    }

    async loadOverviewStats() {
        try {
            const response = await axios.get(`${this.apiBase}/users`);
            const users = response.data;
            
            // Calculate statistics
            const totalUsers = users.length;
            const avgRiskTolerance = users.reduce((sum, user) => sum + (user.risk_tolerance || 0.5), 0) / totalUsers;
            const avgEngagement = users.reduce((sum, user) => sum + (user.engagement_level || 0.5), 0) / totalUsers;
            
            // Update DOM
            document.getElementById('totalUsers').textContent = totalUsers;
            document.getElementById('activeSessions').textContent = users.filter(u => u.total_sessions > 1).length;
            document.getElementById('avgRiskTolerance').textContent = avgRiskTolerance.toFixed(2);
            document.getElementById('engagementRate').textContent = `${Math.round(avgEngagement * 100)}%`;
        } catch (error) {
            console.error('Error loading overview stats:', error);
        }
    }

    async loadUserProfile() {
        this.showLoading(true);
        try {
            const response = await axios.get(`${this.apiBase}/profile/${this.currentUser}`);
            const profile = response.data;
            
            if (profile) {
                this.updateProfileMetrics(profile);
                this.updateBehavioralRadar(profile);
            } else {
                // Create profile if none exists
                await this.analyzeCurrentUser();
            }
        } catch (error) {
            console.error('Error loading user profile:', error);
            this.showToast('Error loading user profile', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    updateProfileMetrics(profile) {
        const metrics = [
            { key: 'risk_tolerance', barId: 'riskToleranceBar', valueId: 'riskToleranceValue' },
            { key: 'decision_speed', barId: 'decisionSpeedBar', valueId: 'decisionSpeedValue' },
            { key: 'resource_efficiency', barId: 'resourceEfficiencyBar', valueId: 'resourceEfficiencyValue' },
            { key: 'strategic_score', barId: 'strategicScoreBar', valueId: 'strategicScoreValue' },
            { key: 'engagement_level', barId: 'engagementLevelBar', valueId: 'engagementLevelValue' },
            { key: 'emotional_responsiveness', barId: 'emotionalResponseBar', valueId: 'emotionalResponseValue' }
        ];

        metrics.forEach(metric => {
            const value = profile[metric.key] || 0.5;
            const bar = document.getElementById(metric.barId);
            const valueSpan = document.getElementById(metric.valueId);
            
            if (bar) bar.style.width = `${value * 100}%`;
            if (valueSpan) valueSpan.textContent = value.toFixed(2);
        });
    }

    updateBehavioralRadar(profile) {
        const ctx = document.getElementById('behavioralRadarChart').getContext('2d');
        
        if (this.charts.behavioralRadar) {
            this.charts.behavioralRadar.destroy();
        }

        this.charts.behavioralRadar = new Chart(ctx, {
            type: 'radar',
            data: {
                labels: ['Risk Tolerance', 'Decision Speed', 'Resource Efficiency', 
                        'Strategic Score', 'Engagement Level', 'Emotional Response'],
                datasets: [{
                    label: 'Behavioral Profile',
                    data: [
                        profile.risk_tolerance || 0.5,
                        profile.decision_speed || 0.5,
                        profile.resource_efficiency || 0.5,
                        profile.strategic_score || 0.5,
                        profile.engagement_level || 0.5,
                        profile.emotional_responsiveness || 0.5
                    ],
                    backgroundColor: 'rgba(102, 126, 234, 0.2)',
                    borderColor: 'rgba(102, 126, 234, 1)',
                    pointBackgroundColor: 'rgba(102, 126, 234, 1)',
                    pointBorderColor: '#fff',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: 'rgba(102, 126, 234, 1)'
                }]
            },
            options: {
                scales: {
                    r: {
                        beginAtZero: true,
                        max: 1,
                        ticks: {
                            stepSize: 0.2
                        }
                    }
                }
            }
        });
    }

async loadUserActions() {
        if (!this.currentUser) return;
        
        this.showLoading(true);
        try {
            const [actionsResponse, usersResponse, influenceResponse] = await Promise.all([
                axios.get(`${this.apiBase}/actions/${this.currentUser}`),
                axios.get(`${this.apiBase}/users`),
                axios.get(`${this.apiBase}/influence-analytics/${this.currentUser}`)
            ]);
            
            const actions = actionsResponse.data;
            const users = usersResponse.data;
            const influenceData = influenceResponse.data;
            
            this.updateActionsTable(actions);
            this.updateDecisionTimeline(actions);
            this.updateResourceManagementChart(actions);
            this.updateEngagementChart(actions);
            this.updateMovementHeatMap(actions);
            this.updateUserComparisonChart(users);
            this.updateInfluenceEffectivenessChart(influenceData.events || []);
        } catch (error) {
            console.error('Error loading user actions:', error);
            this.showToast('Error loading user actions', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    updateActionsTable(actions) {
        const tableBody = document.getElementById('actionsTableBody');
        
        if (actions.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="4">No actions to display...</td></tr>';
            return;
        }

        tableBody.innerHTML = actions.slice(0, 50).map(action => `
            <tr>
                <td>${new Date(action.timestamp).toLocaleString()}</td>
                <td><span class="action-type-badge">${action.action_type}</span></td>
                <td>${this.formatActionDetails(action.action_data)}</td>
                <td>${action.action_data.hexX !== undefined ? `(${action.action_data.hexX}, ${action.action_data.hexY})` : 'N/A'}</td>
            </tr>
        `).join('');

        // Add some CSS for action type badges
        const style = document.createElement('style');
        style.textContent = `
            .action-type-badge {
                background: linear-gradient(135deg, #667eea, #764ba2);
                color: white;
                padding: 0.2rem 0.5rem;
                border-radius: 12px;
                font-size: 0.8rem;
                font-weight: 600;
            }
        `;
        document.head.appendChild(style);
    }

    formatActionDetails(actionData) {
        if (!actionData || !actionData.details) return 'N/A';
        
        const details = actionData.details;
        
        // Format different action types
        if (details.includes('Move')) {
            return `Player movement`;
        } else if (details.includes('Built Village')) {
            return '🏘️ Constructed a village';
        } else if (details.includes('Found')) {
            return `💰 Found resources`;
        } else if (details.includes('Spent')) {
            return `💸 Spent resources`;
        } else if (details.includes('GAME WON')) {
            return '🎉 Victory achieved';
        } else if (details.includes('GAME LOST')) {
            return '😞 Game over';
        }
        
        return details.substring(0, 100);
    }

    updateDecisionTimeline(actions) {
        const ctx = document.getElementById('decisionTimelineChart').getContext('2d');
        
        if (this.charts.decisionTimeline) {
            this.charts.decisionTimeline.destroy();
        }

        const decisionActions = actions.filter(a => a.action_type === 'DecisionTiming');
        const timeline = decisionActions.map((action, index) => ({
            x: index,
            y: parseFloat(action.action_data.details?.split(':')[1]) || 0
        }));

        this.charts.decisionTimeline = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [{
                    label: 'Decision Time (seconds)',
                    data: timeline,
                    borderColor: 'rgba(102, 126, 234, 1)',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                scales: {
                    x: {
                        type: 'linear',
                        position: 'bottom',
                        title: {
                            display: true,
                            text: 'Decision Number'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Time (seconds)'
                        }
                    }
                }
            }
        });
    }

    updateResourceManagementChart(actions) {
        const ctx = document.getElementById('resourceManagementChart').getContext('2d');
        
        if (this.charts.resourceManagement) {
            this.charts.resourceManagement.destroy();
        }

        const resourceActions = actions.filter(a => a.action_type === 'ResourceManagement');
        const gained = resourceActions.filter(a => a.action_data.details?.includes('Found')).length;
        const spent = resourceActions.filter(a => a.action_data.details?.includes('Spent')).length;

        this.charts.resourceManagement = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Resources Gained', 'Resources Spent'],
                datasets: [{
                    data: [gained, spent],
                    backgroundColor: [
                        'rgba(76, 175, 80, 0.8)',
                        'rgba(244, 67, 54, 0.8)'
                    ],
                    borderColor: [
                        'rgba(76, 175, 80, 1)',
                        'rgba(244, 67, 54, 1)'
                    ],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }

    updateEngagementChart(actions) {
        const ctx = document.getElementById('engagementChart').getContext('2d');
        
        if (this.charts.engagement) {
            this.charts.engagement.destroy();
        }

        // Group actions by hour to show engagement patterns
        const hourlyActions = {};
        actions.forEach(action => {
            const hour = new Date(action.timestamp).getHours();
            hourlyActions[hour] = (hourlyActions[hour] || 0) + 1;
        });

        const labels = Array.from({length: 24}, (_, i) => `${i}:00`);
        const data = Array.from({length: 24}, (_, i) => hourlyActions[i] || 0);

        this.charts.engagement = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Actions per Hour',
                    data: data,
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                    borderColor: 'rgba(102, 126, 234, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Number of Actions'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Hour of Day'
                        }
                    }
                }
            }
        });
    }

    async analyzeCurrentUser() {
        if (!this.currentUser) return;
        
        this.showLoading(true);
        try {
            await axios.post(`${this.apiBase}/analyze/${this.currentUser}`);
            await this.loadUserProfile();
            this.showToast('User analysis completed', 'success');
        } catch (error) {
            console.error('Error analyzing user:', error);
            this.showToast('Error analyzing user', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    async generateUserInsights() {
        if (!this.currentUser) return;
        
        try {
            const response = await axios.get(`${this.apiBase}/insights/${this.currentUser}`);
            const insights = response.data;
            
            const insightsDiv = document.getElementById('userInsights');
            insightsDiv.innerHTML = `
                <div class="insight-item">
                    <strong>Player Type:</strong> ${insights.player_type}
                </div>
                ${insights.strengths.length > 0 ? `
                    <div class="insight-item">
                        <strong>Strengths:</strong>
                        <ul>
                            ${insights.strengths.map(s => `<li>${s}</li>`).join('')}
                        </ul>
                    </div>
                ` : ''}
                ${insights.weaknesses.length > 0 ? `
                    <div class="insight-item">
                        <strong>Areas for Improvement:</strong>
                        <ul>
                            ${insights.weaknesses.map(w => `<li>${w}</li>`).join('')}
                        </ul>
                    </div>
                ` : ''}
                ${insights.recommendations.length > 0 ? `
                    <div class="insight-item">
                        <strong>Recommendations:</strong>
                        <ul>
                            ${insights.recommendations.map(r => `<li>${r}</li>`).join('')}
                        </ul>
                    </div>
                ` : ''}
            `;
        } catch (error) {
            console.error('Error generating insights:', error);
            document.getElementById('userInsights').innerHTML = '<p>Error generating insights...</p>';
        }
    }

    async generateInfluenceStrategy() {
        if (!this.currentUser) return;
        
        this.showLoading(true);
        try {
            const response = await axios.get(`${this.apiBase}/influence-strategy/${this.currentUser}?context=game_interaction`);
            const strategy = response.data;
            
            const strategyDiv = document.getElementById('currentStrategy');
            strategyDiv.innerHTML = `
                <div class="strategy-details">
                    <h4>${strategy.influenceType.charAt(0).toUpperCase() + strategy.influenceType.slice(1)} Strategy</h4>
                    <p><strong>Message:</strong> ${strategy.message}</p>
                    <p><strong>Strength:</strong> ${strategy.strength.toFixed(2)}</p>
                    <p><strong>Expected Effectiveness:</strong> ${(strategy.strength * 0.7).toFixed(2)}</p>
                </div>
            `;
            
            // Update influence analytics
            this.updateInfluenceAnalytics();
            
            this.showToast('Influence strategy generated', 'success');
        } catch (error) {
            console.error('Error generating strategy:', error);
            this.showToast('Error generating strategy', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    async updateInfluenceAnalytics() {
        if (!this.currentUser) return;
        
        try {
            const response = await axios.get(`${this.apiBase}/influence-analytics/${this.currentUser}`);
            const analytics = response.data;
            
            document.getElementById('acceptanceRate').textContent = `${Math.round((analytics.effectivenessScore || 0) * 100)}%`;
            document.getElementById('effectivenessScore').textContent = (analytics.effectivenessScore || 0).toFixed(2);
            document.getElementById('susceptibilityTrend').textContent = analytics.susceptibilityTrend || 'Stable';
        } catch (error) {
            console.error('Error loading influence analytics:', error);
        }
    }

    filterActions(actionType) {
        const rows = document.querySelectorAll('#actionsTableBody tr');
        
        rows.forEach(row => {
            if (actionType === '') {
                row.style.display = '';
            } else {
                const actionTypeCell = row.cells[1].textContent;
                row.style.display = actionTypeCell.includes(actionType) ? '' : 'none';
            }
        });
    }

    async exportData() {
        try {
            const response = await axios.get(`${this.apiBase}/users`);
            const users = response.data;
            
            const csv = this.convertToCSV(users);
            this.downloadCSV(csv, 'behavioral_data.csv');
            
            this.showToast('Data exported successfully', 'success');
        } catch (error) {
            console.error('Error exporting data:', error);
            this.showToast('Error exporting data', 'error');
        }
    }

    convertToCSV(data) {
        const headers = Object.keys(data[0]);
        const csvContent = [
            headers.join(','),
            ...data.map(row => headers.map(header => `"${row[header] || ''}"`).join(','))
        ].join('\n');
        
        return csvContent;
    }

    downloadCSV(csv, filename) {
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        window.URL.revokeObjectURL(url);
    }

    showLoading(show) {
        const overlay = document.getElementById('loadingOverlay');
        if (show) {
            overlay.classList.remove('hidden');
        } else {
            overlay.classList.add('hidden');
        }
    }

    showToast(message, type = 'info') {
        const container = document.getElementById('toastContainer');
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        
        const icon = type === 'success' ? '✅' : type === 'error' ? '❌' : 'ℹ️';
        toast.innerHTML = `<span>${icon}</span><span>${message}</span>`;
        
        container.appendChild(toast);
        
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease forwards';
            setTimeout(() => {
                container.removeChild(toast);
            }, 300);
        }, 3000);
    }

    // Heat Map for Player Movement
    updateMovementHeatMap(actions) {
        const ctx = document.getElementById('movementHeatMap').getContext('2d');
        
        if (this.charts.movementHeatMap) {
            this.charts.movementHeatMap.destroy();
        }

        // Extract movement coordinates
        const movements = actions.filter(a => a.action_type === 'Move' && a.action_data.hexX !== undefined);
        const heatData = {};
        
        movements.forEach(movement => {
            const key = `${movement.action_data.hexX},${movement.action_data.hexY}`;
            heatData[key] = (heatData[key] || 0) + 1;
        });

        // Create heat map grid data
        const gridSize = 10;
        const heatMapData = [];
        for (let x = 0; x < gridSize; x++) {
            for (let y = 0; y < gridSize; y++) {
                const key = `${x},${y}`;
                heatMapData.push({
                    x: x,
                    y: y,
                    v: heatData[key] || 0
                });
            }
        }

        this.charts.movementHeatMap = new Chart(ctx, {
            type: 'bubble',
            data: {
                datasets: [{
                    label: 'Movement Heat Map',
                    data: heatMapData,
                    backgroundColor: 'rgba(255, 99, 132, 0.6)',
                    borderColor: 'rgba(255, 99, 132, 1)'
                }]
            },
            options: {
                scales: {
                    x: {
                        min: 0,
                        max: gridSize,
                        title: {
                            display: true,
                            text: 'X Coordinate'
                        }
                    },
                    y: {
                        min: 0,
                        max: gridSize,
                        title: {
                            display: true,
                            text: 'Y Coordinate'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `Position (${context.raw.x}, ${context.raw.y}): ${context.raw.v} movements`;
                            }
                        }
                    }
                }
            }
        });
    }

    // User Profile Comparison
    updateUserComparisonChart(users) {
        const ctx = document.getElementById('userComparisonChart').getContext('2d');
        
        if (this.charts.userComparison) {
            this.charts.userComparison.destroy();
        }

        // Compare current user with averages
        const currentUser = users.find(u => u.uid === this.currentUser);
        if (!currentUser) return;

        const avgRisk = users.reduce((sum, u) => sum + (u.risk_tolerance || 0.5), 0) / users.length;
        const avgEngagement = users.reduce((sum, u) => sum + (u.engagement_level || 0.5), 0) / users.length;
        const avgStrategic = users.reduce((sum, u) => sum + (u.strategic_score || 0.5), 0) / users.length;

        this.charts.userComparison = new Chart(ctx, {
            type: 'radar',
            data: {
                labels: ['Risk Tolerance', 'Engagement Level', 'Strategic Score', 'Decision Speed', 'Resource Efficiency'],
                datasets: [
                    {
                        label: 'Current User',
                        data: [
                            currentUser.risk_tolerance || 0.5,
                            currentUser.engagement_level || 0.5,
                            currentUser.strategic_score || 0.5,
                            currentUser.decision_speed || 0.5,
                            currentUser.resource_efficiency || 0.5
                        ],
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        borderColor: 'rgba(255, 99, 132, 1)',
                        pointBackgroundColor: 'rgba(255, 99, 132, 1)'
                    },
                    {
                        label: 'Average User',
                        data: [avgRisk, avgEngagement, avgStrategic, 0.5, 0.5],
                        backgroundColor: 'rgba(102, 126, 234, 0.2)',
                        borderColor: 'rgba(102, 126, 234, 1)',
                        pointBackgroundColor: 'rgba(102, 126, 234, 1)'
                    }
                ]
            },
            options: {
                scales: {
                    r: {
                        beginAtZero: true,
                        max: 1,
                        ticks: {
                            stepSize: 0.2
                        }
                    }
                }
            }
        });
    }

    // Influence Effectiveness Visualization
    updateInfluenceEffectivenessChart(influenceData) {
        const ctx = document.getElementById('influenceEffectivenessChart').getContext('2d');
        
        if (this.charts.influenceEffectiveness) {
            this.charts.influenceEffectiveness.destroy();
        }

        const labels = [...new Set(influenceData.map(d => d.influence_type))];
        const effectivenessByType = {};
        
        labels.forEach(type => {
            const typeData = influenceData.filter(d => d.influence_type === type);
            effectivenessByType[type] = typeData.reduce((sum, d) => sum + (d.effectiveness_score || 0), 0) / typeData.length;
        });

        this.charts.influenceEffectiveness = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Influence Effectiveness',
                    data: labels.map(type => effectivenessByType[type]),
                    backgroundColor: 'rgba(75, 192, 192, 0.8)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 1,
                        title: {
                            display: true,
                            text: 'Effectiveness Score'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Influence Type'
                        }
                    }
                }
            }
        });
    }
}

// Add slide out animation
const style = document.createElement('style');
style.textContent = `
    @keyframes slideOut {
        to {
            transform: translateX(120%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new BehavioralDashboard();
});