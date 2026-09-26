shader_type canvas_item;

// Convierte el sprite a blanco y negro. Se usa como material cuando una
// propiedad esta hipotecada (tanto en el icono del dueño sobre el tablero
// como en las tarjetas de la lista de propiedades)

void fragment() {
	// Color original del pixel
	vec4 tex_color = texture(TEXTURE, UV);

	// Formula estandar de luminancia para pasar de color a gris
	float gris = dot(tex_color.rgb, vec3(0.299, 0.587, 0.114));

	// Se mantiene el mismo canal alpha (para que la transparencia no cambie)
	COLOR = vec4(vec3(gris), tex_color.a);
}
