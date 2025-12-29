shader_type spatial;

uniform sampler2D heatmap_gradient;
uniform float heat_value : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    // Use heat value to sample gradient
    vec3 heat_color = texture(heatmap_gradient, vec2(heat_value, 0.5)).rgb;
    
    // Add some noise for variation
    float noise = sin(UV.x * 20.0) * cos(UV.y * 20.0) * 0.1;
    
    ALBEDO = heat_color + vec3(noise);
    EMISSION = heat_color * 0.3; // Slight glow based on heat
}
