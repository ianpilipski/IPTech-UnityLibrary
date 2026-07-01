import Foundation
import DeclaredAgeRange
import os.log

public typealias IPTechAgeRangeUnityPlugin_isEligibleForAgeFeaturesCallback = @convention(c) (Int, Int, UnsafeMutablePointer<CChar>?) -> Void
public typealias IPTechAgeRangeUnityPlugin_getAgeRangeCallback = @convention(c) (Int, Int, Int, Int, Int, UnsafeMutablePointer<CChar>?) -> Void

@_cdecl("IPTechAgeRangeUnityPlugin_isEligibleForAgeFeatures")
public func IPTechAgeRangeUnityPlugin_isEligibleForAgeFeatures(callerId: Int, callback: IPTechAgeRangeUnityPlugin_isEligibleForAgeFeaturesCallback?) -> Void {
    guard let nonNilCallback = callback else {
        IPAgeRangeUnityPlugin.shared.logError("Callback is nil, not proceeding with call")
        return
    }
    let callbackWrapper = CallbackWrapperBool(callerId: callerId, callback: nonNilCallback)
    IPAgeRangeUnityPlugin.shared.isEligibleForAgeFeatures(callback: callbackWrapper)
}

@_cdecl("IPTechAgeRangeUnityPlugin_getAgeRange")
public func IPTechAgeRangeUnityPlugin_getAgeRange(
    callerId: Int,
    requiredMinAge: Int,
    additionalMinAge1: Int,
    additionalMinAge2: Int,
    callback: IPTechAgeRangeUnityPlugin_getAgeRangeCallback?)
{
    guard let nonNilCallback = callback else {
        IPAgeRangeUnityPlugin.shared.logError("Callback is nil, not proceeding with call")
        return
    }
    
    let callbackWrapper = CallbackWrapper(callerId: callerId, callback: nonNilCallback)
    IPAgeRangeUnityPlugin.shared.getAgeRange(
        requiredMinAge: requiredMinAge,
        additionalMinAge1: additionalMinAge1,
        additionalMinAge2: additionalMinAge2,
        callback: callbackWrapper);
}

@_cdecl("IPTechAgeRangeUnityPlugin_freeString")
public func IPTechAgeRangeUnityPlugin_freeString(_ stringPtr: UnsafeMutablePointer<CChar>?) -> Void {
    guard let ptr = stringPtr else {return }
    ptr.deallocate()
}

// this creates a new c style string on the heap, that will need to be freed later
func swiftToCString(_ string: String) -> UnsafeMutablePointer<CChar>? {
    let data = string.data(using: .utf8)! + Data([0])
    let count = data.count
    let ptr : UnsafeMutablePointer<CChar> = .allocate(capacity: count)
    ptr.withMemoryRebound(to: UInt8.self, capacity: count) { pointer in
        data.copyBytes(to: pointer, count: count)
    }
    return ptr
}

struct IPTechAgeRangeUnityPlugin_getAgeRangeResult {
    public var callerId: Int
    public var status: Int
    public var minAge: Int
}

enum CallStatus: Int {
    case Error = 0,
    Success = 1,
    UserDeclined = 2,
    UnsupportedPlatformVersion = 3
}

class CallbackWrapperBool 
{
    let callerId: Int
    let callback: IPAgeRangeUnityPlugin_isEligibleForAgeFeaturesCallback
    
    private var strongSelf: CallbackWrapperBool?
    
    init(callerId: Int, callback: @escaping IPAgeRangeUnityPlugin_isEligibleForAgeFeaturesCallback) {
        self.callback = callback
        self.callerId = callerId
        self.strongSelf = self
    }
    
    private func release() {
        self.strongSelf = nil
    }
    
    func ReturnResult(isEligible: Bool) {
        callback(callerId, isEligible ? 1 : 0, nil)
        release()
    }

    func ReturnError(_ error: String) {
        print("Error: \(error)")
        callback(callerId, -1, swiftToCString(error))
        release()
    }
    
    func ReturnPlatformNotSupported() {
        callback(callerId, -2, nil)
        release()
    }
}

class CallbackWrapper
{
    let callerId: Int
    let callback: IPAgeRangeUnityPlugin_getAgeRangeCallback
    
    private var strongSelf: CallbackWrapper?
    
    init(callerId: Int, callback: @escaping IPAgeRangeUnityPlugin_getAgeRangeCallback) {
        self.callback = callback
        self.callerId = callerId
        self.strongSelf = self
    }
    
    private func release() {
        self.strongSelf = nil
    }
    
    @available(iOS 26.0, *)
    func Success(callStatus: CallStatus, ageRange: AgeRangeService.AgeRange?) {
        let lowerBound = ageRange?.lowerBound ?? -1;
        let upperBound = ageRange?.upperBound ?? -1;
        let ageDeclaration = convertAgeRangeDeclarationToCallbackInt(ageRange?.ageRangeDeclaration)
        callback(callerId, callStatus.rawValue, lowerBound, upperBound, ageDeclaration, nil)
        release()
    }
    
    func ErrorPlatformNotSupported() {
        callback(callerId, CallStatus.UnsupportedPlatformVersion.rawValue, 0, 0, 0, nil)
        release()
    }
    
    func Error(_ error: String) {
        print("Error: \(error)")
        callback(callerId, CallStatus.Error.rawValue, 0, 0, 0, swiftToCString(error))
        release()
    }
    
    @available(iOS 26.0, *)
    private func convertAgeRangeDeclarationToCallbackInt(_ ageRangeDeclaration: AgeRangeService.AgeRangeDeclaration?) -> Int {
        
        switch ageRangeDeclaration {
        case .selfDeclared:
            return 1
        case .guardianDeclared:
            return 2
        case .none:
            return 0
        @unknown default:
            return 0
        }
    }
}

class IPTechAgeRangeUnityPlugin {
    static let shared = IPTechAgeRangeUnityPlugin()
    
    private init() {
        // perform any init or config here
    }
    
    func logError(_ message: String) {
        if #available(iOS 14.0, *) {
            let logger = Logger(subsystem: "platform-tech.agerange", category: "IPTechAgeRangeUnityPlugin")
            logger.error("\(message)");
        }
    }
    
    private func getViewController() -> UIViewController? {
        if let unityFramework = UnityFramework.getInstance() {
            let unityAppController = unityFramework.appController()
            return unityAppController?.rootViewController
        }
        return nil
    }

    func isEligibleForAgeFeatures(callback: CallbackWrapperBool) -> Void {
        Task { [callback] in
            do {
                var isEligible: Bool = false
                if #available(iOS 26.2, *) {
                    do {
                        isEligible = try await AgeRangeService.shared.isEligibleForAgeFeatures
                        callback.ReturnResult(isEligible: isEligible)
                        return;
                    } catch let error as AgeRangeService.Error {
                        if(error == .notAvailable) {
                            callback.ReturnError("notAvailable")
                            return;
                        }
                        throw error
                    }
                }
                callback.ReturnPlatformNotSupported()
            } catch let error {
                callback.ReturnError("Unexpected error: \(error.localizedDescription)")
            }
        }
    }
    
    func getAgeRange(
        requiredMinAge: Int, additionalMinAge1: Int, additionalMinAge2: Int,
        callback: CallbackWrapper) {
        Task { [callback] in
            do {
                if #available(iOS 26.0, *) {
                    do {
                        guard let viewController = getViewController() else {
                            callback.Error("[IPAgeRangeUnityPlugin] Could not get a UIViewController to present the AgeRangeService")
                            return;
                        }
                        
                        let threshold2 = additionalMinAge1 > 0 ? additionalMinAge1 : nil
                        let threshold3 = additionalMinAge2 > 0 ? additionalMinAge2 : nil
                        
                        let response : AgeRangeService.Response = try await AgeRangeService.shared.requestAgeRange(
                            ageGates: requiredMinAge, threshold2, threshold3, in: viewController)
                        
                        switch response{
                        case .sharing(range: let ageRange):
                            callback.Success(callStatus: CallStatus.Success, ageRange: ageRange)
                        case .declinedSharing:
                            callback.Success(callStatus: CallStatus.UserDeclined, ageRange: nil)
                        @unknown default:
                            callback.Error("[IPAgeRangeUnityPlugin] AgeRangeService.Response had an unhandled value in the Unknown case")
                        }
                    } catch let ageRangeError as AgeRangeService.Error {
                        switch ageRangeError {
                        case .notAvailable:
                            callback.Error("notAvailable")
                        case .invalidRequest:
                            callback.Error("invalidRequest: Apple's AgeRangeService rejected the age gates (\(requiredMinAge), \(additionalMinAge1), \(additionalMinAge2)). Age gates must be between 1 and 18 and at least 2 years apart.")
                        @unknown default:
                            callback.Error("Unknown AgeRangeService error: \(ageRangeError.localizedDescription)")
                        }
                    }
                    return;
                }
                callback.ErrorPlatformNotSupported()
            } catch let e {
                callback.Error("Unexpected error: \(e.localizedDescription)")
            }
        }
    }
}
