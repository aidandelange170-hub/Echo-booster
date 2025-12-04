"""
Level 1: Basic Authentication Security
- Username/Password validation
- Rate limiting
- Session management
"""

import hashlib
import time
from typing import Dict, Optional

class BasicAuthSecurity:
    def __init__(self):
        self.users = {
            "admin": self._hash_password("secure_password_123"),
            "user": self._hash_password("user_password_456")
        }
        self.failed_attempts = {}
        self.lockout_time = 300  # 5 minutes lockout
        
    def _hash_password(self, password: str) -> str:
        """Hash password using SHA-256"""
        return hashlib.sha256(password.encode()).hexdigest()
    
    def authenticate(self, username: str, password: str) -> bool:
        """Authenticate user with rate limiting"""
        current_time = time.time()
        
        # Check if user is locked out
        if username in self.failed_attempts:
            last_attempt, attempts = self.failed_attempts[username]
            if attempts >= 3 and current_time - last_attempt < self.lockout_time:
                print(f"User {username} is locked out for {self.lockout_time} seconds")
                return False
        
        # Validate credentials
        expected_hash = self.users.get(username)
        if expected_hash and expected_hash == self._hash_password(password):
            # Reset failed attempts on successful login
            if username in self.failed_attempts:
                del self.failed_attempts[username]
            return True
        
        # Record failed attempt
        if username not in self.failed_attempts:
            self.failed_attempts[username] = (current_time, 1)
        else:
            last_time, attempts = self.failed_attempts[username]
            self.failed_attempts[username] = (current_time, attempts + 1)
        
        return False

if __name__ == "__main__":
    auth = BasicAuthSecurity()
    print("Level 1 Security: Basic Authentication System")
    print("Testing authentication...")
    print(auth.authenticate("admin", "secure_password_123"))  # Should return True
    print(auth.authenticate("admin", "wrong_password"))       # Should return False