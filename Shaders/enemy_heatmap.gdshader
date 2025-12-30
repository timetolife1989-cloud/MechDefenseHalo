shader_type spatial;

uniform float heat_value : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    // Generate heat color from cool (blue) to hot (red) gradient
    vec3 cool_color = vec3(0.0, 0.5, 1.0);  // Blue
    vec3 warm_color = vec3(1.0, 0.5, 0.0);  // Orange
    vec3 hot_color = vec3(1.0, 0.0, 0.0);   // Red
    
    // Interpolate between colors based on heat value
    vec3 heat_color;
    if (heat_value < 0.5) {
        heat_color = mix(cool_color, warm_color, heat_value * 2.0);
    } else {
        heat_color = mix(warm_color, hot_color, (heat_value - 0.5) * 2.0);
    }
    
    // Add some noise for variation
    float noise = sin(UV.x * 20.0) * cos(UV.y * 20.0) * 0.1;
    
    ALBEDO = heat_color + vec3(noise);
    EMISSION = heat_color * 0.3; // Slight glow based on heat
}
