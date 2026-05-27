mergeInto(LibraryManager.library, {

    RegisterBiometric: function(userIdPtr, userNamePtr) {
        var userId = UTF8ToString(userIdPtr);
        var userName = UTF8ToString(userNamePtr);
        
        if (!window.PublicKeyCredential) {
            SendMessage('BiometricManager', 'OnBiometricNotSupported', 
                'WebAuthn niet ondersteund');
            return;
        }

        var publicKeyCredentialCreationOptions = {
            challenge: new Uint8Array(32),
            rp: {
                name: "SimVault",
                id: window.location.hostname
            },
            user: {
                id: new TextEncoder().encode(userId),
                name: userName,
                displayName: userName
            },
            pubKeyCredParams: [
                { alg: -7, type: "public-key" },
                { alg: -257, type: "public-key" }
            ],
            authenticatorSelection: {
                authenticatorAttachment: "platform",
                userVerification: "required"
            },
            timeout: 60000
        };

        window.crypto.getRandomValues(
            publicKeyCredentialCreationOptions.challenge);

        navigator.credentials.create({
            publicKey: publicKeyCredentialCreationOptions
        }).then(function(credential) {
            localStorage.setItem('simvault_credential_id', 
                btoa(String.fromCharCode(...new Uint8Array(credential.rawId))));
            SendMessage('BiometricManager', 'OnBiometricRegistered', 'success');
        }).catch(function(error) {
            SendMessage('BiometricManager', 'OnBiometricError', error.message);
        });
    },

    AuthenticateWithBiometric: function() {
        if (!window.PublicKeyCredential) {
            SendMessage('BiometricManager', 'OnBiometricNotSupported',
                'WebAuthn niet ondersteund');
            return;
        }

        var credentialId = localStorage.getItem('simvault_credential_id');
        if (!credentialId) {
            SendMessage('BiometricManager', 'OnBiometricError', 
                'Geen biometrics geregistreerd');
            return;
        }

        var rawId = Uint8Array.from(atob(credentialId), c => c.charCodeAt(0));

        var publicKeyCredentialRequestOptions = {
            challenge: new Uint8Array(32),
            allowCredentials: [{
                id: rawId,
                type: 'public-key',
                transports: ['internal']
            }],
            userVerification: "required",
            timeout: 60000
        };

        window.crypto.getRandomValues(
            publicKeyCredentialRequestOptions.challenge);

        navigator.credentials.get({
            publicKey: publicKeyCredentialRequestOptions
        }).then(function(assertion) {
            SendMessage('BiometricManager', 'OnBiometricAuthenticated', 
                'success');
        }).catch(function(error) {
            SendMessage('BiometricManager', 'OnBiometricError', error.message);
        });
    },

    IsBiometricAvailable: function() {
        if (!window.PublicKeyCredential) return false;
        var credentialId = localStorage.getItem('simvault_credential_id');
        console.log('[Biometric] credentialId:', credentialId);
        return credentialId !== null;
    },

    InitKeyboardDetection: function() {
    if (window.visualViewport) {
        // Moderne aanpak via visualViewport API
        window.visualViewport.addEventListener('resize', function() {
            var ratio = window.visualViewport.height / window.screen.height;
            
            if (ratio < 0.75) {
                SendMessage('KeyboardDetector', 'OnKeyboardShown', '1');
            } else {
                SendMessage('KeyboardDetector', 'OnKeyboardHidden', '1');
            }
        });
    } else {
        // Fallback voor oudere browsers
        var originalHeight = window.innerHeight;
        window.addEventListener('resize', function() {
            var ratio = window.innerHeight / originalHeight;
            if (ratio < 0.75) {
                SendMessage('KeyboardDetector', 'OnKeyboardShown', '1');
            } else {
                SendMessage('KeyboardDetector', 'OnKeyboardHidden', '1');
            }
        });
    }
}

});