import Foundation
import Vision

let screenshot = URL(fileURLWithPath: CommandLine.arguments[1])
let request = VNRecognizeTextRequest()
request.recognitionLevel = .accurate
request.recognitionLanguages = ["en-US"]
try VNImageRequestHandler(url: screenshot).perform([request])

let visibleText = (request.results ?? []).compactMap { $0.topCandidates(1).first?.string }
print(visibleText.joined(separator: "\n"))

let expectedLabels = ["GymApp", "Email", "Password", "Sign in"]
let missingLabels = expectedLabels.filter { expected in
    !visibleText.contains { $0.caseInsensitiveCompare(expected) == .orderedSame }
}
if !missingLabels.isEmpty {
    print("Missing login labels: \(missingLabels.joined(separator: ", "))")
    exit(1)
}
