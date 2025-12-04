"""
Level 2: Two-Factor Authentication Security
- SMS/Email verification
- Time-based tokens (TOTP)
- Backup codes
"""

import hashlib
import time
import random
from datetime import datetime, timedelta
from typing import Dict, Optional

class TwoFactorSecurity:
    def __init__(self):
        self.totp_secrets = {
            "admin": "JBSWY3DPEHPK3PXP",
            "user": "JBSWY3DPEHPK3PYQ"
        }
        self.verification_codes = {}
        self.backup_codes = {
            "admin": ["123456", "234567", "345678"],
            "user": ["456789", "567890", "678901"]
        }
        
    def generate_totp(self, secret: str, period: int = 30) -> str:
        """Generate Time-based One-Time Password"""
        # Simplified TOTP algorithm for demonstration
        counter = int(time.time() // period)
        # In a real system, this would use HMAC-SHA1 with the secret
        totp = f"{hash(secret + str(counter)) % 1000000:06d}"
        return totp
    
    def send_verification_code(self, username: str) -> str:
        """Send verification code to user"""
        code = f"{random.randint(100000, 999999):06d}"
        self.verification_codes[username] = {
            "code": code,
            "timestamp": time.time(),
            "attempts": 0
        }
        print(f"Verification code sent to {username}: {code}")
        return code
    
    def verify_totp(self, username: str, token: str) -> bool:
        """Verify the TOTP token"""
        if username not in self.totp_secrets:
            return False
        
        expected_token = self.generate_totp(self.totp_secrets[username])
        return token == expected_token
    
    def verify_sms_code(self, username: str, code: str) -> bool:
        """Verify SMS verification code"""
        if username not in self.verification_codes:
            return False
        
        code_info = self.verification_codes[username]
        
        # Check if code is expired (5 minutes)
        if time.time() - code_info["timestamp"] > 300:
            del self.verification_codes[username]
            return False
        
        # Check if too many attempts
        if code_info["attempts"] >= 3:
            del self.verification_codes[username]
            return False
        
        # Verify code
        if code == code_info["code"]:
            del self.verification_codes[username]
            return True
        else:
            code_info["attempts"] += 1
            return False
    
    def verify_backup_code(self, username: str, code: str) -> bool:
        """Verify backup code"""
        if username in self.backup_codes:
            if code in self.backup_codes[username]:
                # Remove used backup code
                self.backup_codes[username].remove(code)
                return True
        return False

if __name__ == "__main__":
    tfa = TwoFactorSecurity()
    print("Level 2 Security: Two-Factor Authentication System")
    
    # Test TOTP
    admin_secret = tfa.totp_secrets["admin"]
    totp = tfa.generate_totp(admin_secret)
    print(f"Generated TOTP: {totp}")
    print(f"TOTP verification: {tfa.verify_totp('admin', totp)}")
    
    # Test SMS verification
    code = tfa.send_verification_code("admin")
    print(f"SMS verification: {tfa.verify_sms_code('admin', code)}")