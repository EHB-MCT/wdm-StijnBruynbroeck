# Unity Build Container - Weapon of Math Destruction
# Builds Unity project for standalone deployment
# Multi-stage build for efficient image size

FROM --platform=linux/amd64 unityci/editor:2021.3.15f1-1.0.0 as builder

# Build arguments
ARG UNITY_VERSION=2021.3.15f1
ARG BUILD_TARGET=StandaloneLinux64

# Install build dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    python3 \
    python3-pip \
    git \
    wget \
    unzip

# Create project directory
WORKDIR /project

# Copy Unity project files
COPY ./ /project/

# Unity license (if needed)
# Add UNITY_LICENSE file if you have one
# COPY ./UNITY_LICENSE /Unity/Unity_lic.ulf

# Build Unity project
RUN /opt/unity/Editor/Unity \
    -batchmode \
    -quit \
    -buildTarget $BUILD_TARGET \
    -projectPath /project \
    -logFile /tmp/build.log \
    -executeMethod BuildScript.PerformBuild

# Build output stage
FROM ubuntu:22.04

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    libstdc++6 \
    libgcc-s1 \
    libx6-4 \
    libxrandr2 \
    libxinerama1 \
    libxcursor1 \
    libxi6 \
    libgl1-mesa-glx

# Create app user
RUN useradd -m -u 1000 unity

# Copy built game from builder stage
COPY --from=builder /project/build /game/

# Set permissions
RUN chown -R unity:unity /game/

# Switch to app user
USER unity

# Expose game port (if needed for networking)
EXPOSE 8080

# Set working directory
WORKDIR /game

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD test -f ./WeaponOfMathDestruction.x86_64 || exit 1

# Run the game
CMD ["./WeaponOfMathDestruction.x86_64"]