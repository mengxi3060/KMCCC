#!/usr/bin/env python3
"""
简单的下载服务器 - 提供 Minecraft 启动器下载页面
"""

import http.server
import socketserver
import os
from pathlib import Path

PORT = 8888

class DownloadHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(Path(__file__).parent), **kwargs)
    
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        super().end_headers()
    
    def do_GET(self):
        if self.path == '/' or self.path == '/index.html':
            self.path = '/download.html'
        return super().do_GET()

if __name__ == '__main__':
    print(f"""
╔════════════════════════════════════════════╗
║   Minecraft 启动器 - 下载服务器             ║
╠════════════════════════════════════════════╣
║  🌐 访问地址: http://localhost:{PORT}        ║
║  📁 监听目录: {os.path.dirname(os.path.abspath(__file__))}
║                                            ║
║  按 Ctrl+C 停止服务器                       ║
╚════════════════════════════════════════════╝
    """)
    
    with socketserver.TCPServer(("", PORT), DownloadHandler) as httpd:
        print(f"服务器已启动！请访问 http://localhost:{PORT}")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\n服务器已停止")
