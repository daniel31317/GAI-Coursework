/*
List references here in Harvard format:

A* psuedocode from the 5 Search Techniques and Pathfinding Lecture (Slide 25). https://caledonianac-my.sharepoint.com/:p:/g/personal/hho2_gcu_ac_uk/ES43Kju9iY1PiJWllxnCpS4B0u2d3NV3w42g3Z_mwNfVpA?e=8XLhqT
 
McNeill, J (2007). Raytracing on a Grid 27 March. Available at: https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html (Accessed: 17 November 2025).

Reynolds, C (1999). Steering Behaviors For Autonomous Characters. Available at: https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf (Accessed: 10 November 2025, 12 November 2025, 29 November 2025, 9 December 2025).

corbettmaths (2017). Shortest Distance of a Point from a Line. Available at: https://www.youtube.com/watch?v=aA15QO4YD6U (Accessed: 9 December 2025).





Ai https://gemini.google.com : I did attempt to use ai to help me with the steering behaviours as that was the main struggle part where I turned to AI
however it just kept telling me to change the transform on multiple occasions so I stopped trying to use it

An example of one of the many useless responses I got




Prompt "how would i make a 2d "agent" rotate slowly around back and fourth by only controlling its steering velocity"


(the weird $\sin(\text{time})$ are just symbols like Ttarget and just dont translate accorss)
Response "This is a common problem in game development and robotics, 
often solved using a technique called Proportional Control or by implementing a simple oscillation function.
Since you can only control the steering velocity (the rate of change of the agent's rotation), 
you need a system that smoothly changes this velocity to slow down and reverse the rotation when it reaches a certain angle limit.
Here is a method using a simple sine function, which naturally provides the smooth, back-and-forth motion you're looking for.🔄 
Method: Sine Wave OscillationA sine wave is perfect for this because its value naturally moves back and forth between a maximum and a minimum, 
and its slope (which relates to the velocity) also changes smoothly, ensuring your agent doesn't abruptly stop and start.1. 
The Core IdeaYou will use a $\sin(\text{time})$ function to determine the agent's target angle (or position in its rotational cycle).
Let:$A$ be the amplitude, which is the maximum angle (in degrees or radians) the agent rotates away from its starting orientation.
$\omega$ (omega) be the angular frequency, which controls the speed of the oscillation.1 A smaller $\omega$ means a slower rotation.
$T$ be the total rotation of the agent (its current orientation).$t$ be the time elapsed since the start of the rotation 
(usually $\text{time}$ in seconds from your game engine).2. Calculating the Target AngleFirst, calculate the desired or target angle 
($T_{\text{target}}$) at the current time $t$:$$T_{\text{target}} = A \cdot \sin(\omega \cdot t)$$This function will make 
$T_{\text{target}}$ smoothly oscillate between $-A$ and $+A$.3. Calculating the Steering VelocityYour agent only takes a steering velocity ($V_{\text{steer}}$) input. 
$V_{\text{steer}}$ is the value you must set to make the agent's current rotation ($T_{\text{current}}$) move toward the $T_{\text{target}}$.
The simplest way to calculate this is using a Proportional Control (P-Control) approach:$$V_{\text{steer}} = K_p \cdot (T_{\text{target}} - T_{\text{current}})$$
Where:$T_{\text{target}} - T_{\text{current}}$ is the angle error (how far the agent is from where it should be).$K_p$ is the proportional gain (a constant number, 
e.g., $K_p \approx 5$ to $10$). This scales the error to a usable steering velocity.This formula means:If $T_{\text{current}}$ is far below $T_{\text{target}}$
(positive error), $V_{\text{steer}}$ will be a large positive number, causing the agent to turn quickly toward the target.If $T_{\text{current}}$ is near $T_{\text{target}}$
(small error), $V_{\text{steer}}$ will be small, causing the agent to turn slowly or stop.If $T_{\text{current}}$ goes past $T_{\text{target}}$ (negative error), 
$V_{\text{steer}}$ will be a negative number, causing the agent to turn back and correct its position.The combination of the oscillating $T_{\text{target}}$ 
and the P-Control $V_{\text{steer}}$ results in the smooth, slow back-and-forth rotation you asked for.🛠️ Implementation Steps (Pseudo-Code)You would run 
this logic in your game engine's main update loop (e.g., Update() or tick() function):// --- Setup/Initialization (Run Once) ---
// Define the total range of rotation (e.g., +/- 45 degrees)
const AMPLITUDE = 45.0 
// Define the speed/period of the rotation. Smaller is slower.
const FREQUENCY = 0.5 
// Define the proportional gain (tune this to make the steering responsive but not shaky)
const PROPORTIONAL_GAIN = 8.0 

// --- Update Loop (Run Every Frame) ---
function Update(time_in_seconds) {

    // 1. Calculate the Target Angle
    let target_angle = AMPLITUDE * sin(FREQUENCY * time_in_seconds)

    // 2. Get the Agent's Current Rotation
    let current_angle = Agent.get_current_rotation() // Assume this is the agent's absolute rotation

    // 3. Calculate the Steering Velocity
    let angle_error = target_angle - current_angle
    let steering_velocity = PROPORTIONAL_GAIN * angle_error

    // 4. Apply the Velocity
    Agent.set_steering_velocity(steering_velocity)
}
This image visually represents the target and current angle relationship. 
The current angle follows the target angle, and the rate at which it follows is your steering velocity.
Would you like a more detailed explanation of how to tune the constants like $K_p$ or the $\omega$ frequency?"

I then prompted "could you implement this for unity"

Response: (Ignoring all the extra talk and just the code it provided)

using UnityEngine;

public class SteeringAgentOscillator : MonoBehaviour
{
    // --- Public Parameters (Visible and Tunable in the Inspector) ---
    
    [Header("Oscillation Settings")]
    [Tooltip("The maximum angle (in degrees) the agent will rotate away from its start position.")]
    public float amplitude = 45f; // A
    [Tooltip("Controls the speed of the back-and-forth rotation. Smaller values mean slower rotation.")]
    public float frequency = 0.5f; // omega (ω)

    [Header("Steering Control (Proportional Gain)")]
    [Tooltip("Scales the angle error into the final steering velocity. Tune this for responsiveness.")]
    public float proportionalGain = 8f; // Kp

    [Tooltip("Optional: Maximum steering speed to prevent overshooting or jerky movement.")]
    public float maxSteeringSpeed = 100f; 

    // --- Private Variables ---
    
    private float startRotationY;

    void Start()
    {
        // Store the initial rotation on the Y-axis (assuming 2D agent facing Z, rotating around Y)
        // If your 2D agent is rotated in the XY plane, you might need transform.localEulerAngles.z
        startRotationY = transform.localEulerAngles.y;
    }

    void Update()
    {
        // 1. Calculate the Target Angle (Relative to the Starting Rotation)
        // Time.time is the time since the start of the game.
        // Mathf.Sin returns a value between -1 and 1.
        float targetAngleOffset = amplitude * Mathf.Sin(frequency * Time.time);

        // The absolute target rotation we want to reach
        float targetRotationY = startRotationY + targetAngleOffset;

        // 2. Get the Agent's Current Rotation
        // Use localEulerAngles for rotations relative to the parent (or world if no parent)
        float currentRotationY = transform.localEulerAngles.y;
        
        // Ensure the rotation values are continuous (e.g., convert 359 to -1)
        // This helper function makes the angle subtraction reliable across the 360/0 degree wrap point.
        float angleError = Mathf.DeltaAngle(currentRotationY, targetRotationY);

        // 3. Calculate the Steering Velocity (Proportional Control)
        // The velocity is proportional to the error
        float steeringVelocity = proportionalGain * angleError;

        // 4. Apply Constraints (Optional but Recommended)
        // Clamp the velocity to prevent extremely fast spinning
        steeringVelocity = Mathf.Clamp(steeringVelocity, -maxSteeringSpeed, maxSteeringSpeed);

        // 5. Apply the Velocity
        // Time.deltaTime ensures the movement is framerate-independent (smooth)
        float rotationStep = steeringVelocity * Time.deltaTime;
        
        // This rotates the agent around its Y-axis by the calculated step.
        transform.Rotate(0, rotationStep, 0, Space.Self);
    }
}


this just ends up rotating the transoform which we are not allowed to do. There is many other example of this happening
i will not include them because I never really used them at all since they weren't useful.


*/