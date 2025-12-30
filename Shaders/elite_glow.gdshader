shader_type spatial;
render_mode blend_add, unshaded;

uniform vec4 glow_color : source_color = vec4(1.0, 0.8, 0.0, 1.0);
uniform float glow_intensity : hint_range(0.0, 5.0) = 2.0;
uniform float pulse_speed : hint_range(0.0, 10.0) = 2.0;

void fragment() {
    // Create pulsing effect
    float pulse = sin(TIME * pulse_speed) * 0.5 + 0.5;
    
    // Edge detection for glow (Fresnel-like effect)
    vec3 view_dir = normalize(VIEW);
    float edge = 1.0 - abs(dot(view_dir, NORMAL));
    edge = pow(edge, 3.0);
    
    // Combine pulse and edge
    float glow = edge * glow_intensity * (0.5 + pulse * 0.5);
    
    ALBEDO = glow_color.rgb;
    EMISSION = glow_color.rgb * glow;
    ALPHA = edge * 0.5;
}
