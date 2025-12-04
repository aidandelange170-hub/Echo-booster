"""
Level 3: Biometric Security
- Fingerprint recognition
- Facial recognition
- Voice pattern analysis
- Behavioral biometrics
"""

import hashlib
import time
import numpy as np
from typing import Dict, List, Optional

class BiometricSecurity:
    def __init__(self):
        self.fingerprint_templates = {
            "admin": self._generate_fingerprint_template("admin_unique_pattern"),
            "user": self._generate_fingerprint_template("user_unique_pattern")
        }
        self.voice_patterns = {
            "admin": self._generate_voice_pattern("admin_voice_sample"),
            "user": self._generate_voice_pattern("user_voice_sample")
        }
        self.face_templates = {
            "admin": self._generate_face_template("admin_face_features"),
            "user": self._generate_face_template("user_face_features")
        }
        self.behavioral_patterns = {
            "admin": {
                "typing_rhythm": [0.2, 0.3, 0.1, 0.4, 0.2],
                "mouse_movement": [1.2, 0.8, 1.5, 0.9, 1.1],
                "login_times": [9, 10, 14, 15]  # hours when usually active
            },
            "user": {
                "typing_rhythm": [0.4, 0.5, 0.2, 0.6, 0.3],
                "mouse_movement": [1.5, 1.2, 1.8, 1.0, 1.3],
                "login_times": [8, 12, 19, 20]
            }
        }
    
    def _generate_fingerprint_template(self, seed: str) -> List[float]:
        """Generate a simulated fingerprint template"""
        np.random.seed(hash(seed) % 2**32)
        return np.random.random(100).tolist()
    
    def _generate_voice_pattern(self, seed: str) -> List[float]:
        """Generate a simulated voice pattern"""
        np.random.seed((hash(seed) + 1) % 2**32)
        return np.random.random(50).tolist()
    
    def _generate_face_template(self, seed: str) -> List[float]:
        """Generate a simulated face template"""
        np.random.seed((hash(seed) + 2) % 2**32)
        return np.random.random(128).tolist()
    
    def verify_fingerprint(self, username: str, input_template: List[float]) -> bool:
        """Verify fingerprint against stored template"""
        if username not in self.fingerprint_templates:
            return False
        
        stored_template = self.fingerprint_templates[username]
        
        # Calculate similarity (simplified)
        similarity = self._calculate_similarity(stored_template, input_template)
        
        # Set threshold for acceptance (95% similarity required)
        return similarity > 0.95
    
    def verify_voice(self, username: str, input_pattern: List[float]) -> bool:
        """Verify voice pattern against stored template"""
        if username not in self.voice_patterns:
            return False
        
        stored_pattern = self.voice_patterns[username]
        similarity = self._calculate_similarity(stored_pattern, input_pattern)
        
        return similarity > 0.92  # 92% similarity required for voice
    
    def verify_face(self, username: str, input_template: List[float]) -> bool:
        """Verify face against stored template"""
        if username not in self.face_templates:
            return False
        
        stored_template = self.face_templates[username]
        similarity = self._calculate_similarity(stored_template, input_template)
        
        return similarity > 0.96  # 96% similarity required for face
    
    def verify_behavioral(self, username: str, typing_rhythm: List[float], 
                         mouse_movement: List[float], login_hour: int) -> bool:
        """Verify behavioral patterns"""
        if username not in self.behavioral_patterns:
            return False
        
        stored = self.behavioral_patterns[username]
        
        typing_similarity = self._calculate_similarity(stored["typing_rhythm"], typing_rhythm)
        mouse_similarity = self._calculate_similarity(stored["mouse_movement"], mouse_movement)
        
        # Check if login time is within normal hours
        time_match = login_hour in stored["login_times"]
        
        # All three behavioral factors must be within acceptable ranges
        return typing_similarity > 0.85 and mouse_similarity > 0.85 and time_match
    
    def _calculate_similarity(self, template1: List[float], template2: List[float]) -> float:
        """Calculate similarity between two templates"""
        if len(template1) != len(template2):
            return 0.0
        
        # Calculate cosine similarity
        dot_product = sum(a * b for a, b in zip(template1, template2))
        magnitude1 = sum(a * a for a in template1) ** 0.5
        magnitude2 = sum(b * b for b in template2) ** 0.5
        
        if magnitude1 == 0 or magnitude2 == 0:
            return 0.0
        
        similarity = dot_product / (magnitude1 * magnitude2)
        return max(0.0, similarity)  # Ensure non-negative result
    
    def enroll_user(self, username: str, fingerprint_data: List[float], 
                   voice_data: List[float], face_data: List[float]):
        """Enroll a new user with biometric data"""
        self.fingerprint_templates[username] = fingerprint_data
        self.voice_patterns[username] = voice_data
        self.face_templates[username] = face_data
        self.behavioral_patterns[username] = {
            "typing_rhythm": [0.3, 0.4, 0.15, 0.5, 0.25],
            "mouse_movement": [1.3, 1.0, 1.6, 0.95, 1.2],
            "login_times": [10, 15, 18]
        }

if __name__ == "__main__":
    bio = BiometricSecurity()
    print("Level 3 Security: Biometric Authentication System")
    
    # Test fingerprint verification
    admin_fingerprint = bio.fingerprint_templates["admin"]
    print(f"Fingerprint verification: {bio.verify_fingerprint('admin', admin_fingerprint)}")
    
    # Test voice verification
    admin_voice = bio.voice_patterns["admin"]
    print(f"Voice verification: {bio.verify_voice('admin', admin_voice)}")
    
    # Test face verification
    admin_face = bio.face_templates["admin"]
    print(f"Face verification: {bio.verify_face('admin', admin_face)}")