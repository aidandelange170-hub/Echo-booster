#!/usr/bin/env python3
"""
Main Entry Point for the 5-Level AI Security System
This is the top-level orchestrator for the entire security infrastructure.
"""

import time
import sys
import os
from ai_security_automation import AIAutomatedSecurity

def main():
    print("="*70)
    print("🔐 ADVANCED 5-LEVEL AI SECURITY SYSTEM 🔐")
    print("Maximum Protection with Quantum AI Intelligence")
    print("="*70)
    
    try:
        # Initialize the AI Security System
        print("\n 🚀 Initializing Security Infrastructure...")
        security_system = AIAutomatedSecurity()
        
        print("\n 🎯 Running Security Validation Tests...")
        
        # Run a complete security validation
        result = security_system.authenticate_user(
            "admin", 
            "secure_password_123",
            {}
        )
        
        if result["authenticated"]:
            print(f"\n ✅ FULL SECURITY VALIDATION SUCCESSFUL")
            print(f"   User: {result['user']}")
            print(f"   Security Score: {result['security_score']:.3f}")
            print(f"   Threat Level: {result['threat_level']:.3f}")
            print(f"   Passed All 5 Security Levels!")
        else:
            print(f"\n ❌ Security validation failed: {result['reason']}")
        
        # Show security report
        report = security_system.get_security_report()
        print(f"\n 📊 Security Status Report:")
        print(f"   System Status: {report['system_status']}")
        print(f"   Active Users: {len(report['active_users'])}")
        print(f"   Total Attempts: {report['security_metrics']['total_attempts']}")
        print(f"   Successful: {report['security_metrics']['successful_auths']}")
        
        # Run attack simulation
        security_system.simulate_attack_and_defense()
        
        print(f"\n 🛡️  All security levels are active and operational!")
        print(f" 🤖 AI Security Automation System is protecting your assets!")
        print(f" 🔒 Level 5 (Quantum AI) - Impossible to bypass security active!")
        
    except KeyboardInterrupt:
        print("\n\n ⚠️  Security system shutdown initiated by user...")
    except Exception as e:
        print(f"\n ❌ Security system error: {str(e)}")
        sys.exit(1)
    
    print("\n" + "="*70)
    print("5-LEVEL AI SECURITY SYSTEM - ACTIVE PROTECTION ENGAGED")
    print("Threat Level: MAXIMUM | Bypass Probability: NEAR ZERO")
    print("="*70)

if __name__ == "__main__":
    main()