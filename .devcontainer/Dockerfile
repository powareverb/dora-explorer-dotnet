# Using this dockerfile allows us to bundle all dependencies into the image, which makes it faster to start the container and ensures that all users have the same environment.
FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04
COPY mise.toml .
COPY --from=jdxcode/mise /usr/local/bin/mise /usr/local/bin/
RUN echo 'eval "$(mise activate bash)"' >> /home/vscode/.bashrc \
    && echo 'eval "$(mise activate zsh)"'  >> /home/vscode/.zshrc
COPY .devcontainer/setup.sh /tmp/setup.sh
RUN chmod +x /tmp/setup.sh && /tmp/setup.sh
USER vscode
