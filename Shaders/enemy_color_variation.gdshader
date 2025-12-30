shader_type spatial;

uniform vec3 base_color : source_color = vec3(1.0, 0.5, 0.2);
uniform float variation_amount : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    vec3 noise = vec3(
        sin(UV.x * 10.0 + TIME),
        cos(UV.y * 10.0),
        sin((UV.x + UV.y) * 5.0)
    ) * variation_amount * 0.2;
    
    ALBEDO = base_color + noise;
}
