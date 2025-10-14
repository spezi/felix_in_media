/*{
  "ISFVSN":"2.0",
  "CATEGORIES":["Warp","Mapping","Transform"],
  "INPUTS":[
    { "NAME":"inputImage", "TYPE":"image" },
    { "NAME":"fitMode", "TYPE":"long", "LABEL":"Fit Mode",
      "VALUES":[0,1,2], "VALUES_LABELS":["Fit (preserve aspect)","Fill (crop)","Stretch"], "DEFAULT":0 },
    { "NAME":"useQuadWarp", "TYPE":"bool", "LABEL":"Use Quad Warp", "DEFAULT":false },
    { "NAME":"corner0", "TYPE":"point2D", "LABEL":"Dest Corner 0 (top-left)", "DEFAULT":[0.0,0.0] },
    { "NAME":"corner1", "TYPE":"point2D", "LABEL":"Dest Corner 1 (top-right)", "DEFAULT":[1.0,0.0] },
    { "NAME":"corner2", "TYPE":"point2D", "LABEL":"Dest Corner 2 (bottom-right)", "DEFAULT":[1.0,1.0] },
    { "NAME":"corner3", "TYPE":"point2D", "LABEL":"Dest Corner 3 (bottom-left)", "DEFAULT":[0.0,1.0] },
    { "NAME":"edgeBleed", "TYPE":"float", "LABEL":"Edge Bleed (px)", "MIN":0.0, "MAX":200.0, "DEFAULT":0.0 },
    { "NAME":"gamma", "TYPE":"float", "LABEL":"Gamma", "MIN":0.1, "MAX":3.0, "DEFAULT":1.0 }
  ]
}*/

vec2 bilinearMap(vec2 uv, vec2 p0, vec2 p1, vec2 p2, vec2 p3) {
    float s = uv.x;
    float t = uv.y;
    vec2 a = mix(p0, p1, s);
    vec2 b = mix(p3, p2, s);
    return mix(a, b, t);
}

vec2 solveUV(vec2 p, vec2 p0, vec2 p1, vec2 p2, vec2 p3) {
    vec2 minp = min(min(p0,p1), min(p2,p3));
    vec2 maxp = max(max(p0,p1), max(p2,p3));
    vec2 uv = vec2((p.x - minp.x) / max(1e-6, (maxp.x - minp.x)),
                   (p.y - minp.y) / max(1e-6, (maxp.y - minp.y)));
    uv = clamp(uv, 0.0, 1.0);

    for (int i = 0; i < 6; i++) {
        vec2 f = bilinearMap(uv, p0, p1, p2, p3) - p;
        float s = uv.x;
        float t = uv.y;
        vec2 dPds = (1.0 - t) * (p1 - p0) + t * (p2 - p3);
        vec2 dPdt = (1.0 - s) * (p3 - p0) + s * (p2 - p1);
        float a = dPds.x;
        float b = dPdt.x;
        float c = dPds.y;
        float d = dPdt.y;
        float det = a*d - b*c;
        if (abs(det) < 1e-6) break;
        vec2 delta = vec2(( d * (-f.x) - b * (-f.y)) / det,
                          (-c * (-f.x) + a * (-f.y)) / det);
        uv += delta;
        uv = clamp(uv, -0.2, 1.2);
        if (length(delta) < 1e-4) break;
    }
    return clamp(uv, 0.0, 1.0);
}

void main() {
    vec2 outUV = gl_FragCoord.xy / RENDERSIZE;
    vec2 inSize = vec2(1920.0, 2160.0);

    vec2 srcUV;

    if (!useQuadWarp) {
        float srcAspect = inSize.x / inSize.y;
        float outAspect = RENDERSIZE.x / RENDERSIZE.y;

        if (fitMode == 2) {
            srcUV = outUV;
        } else if ((fitMode == 0 && srcAspect > outAspect) || (fitMode == 1 && srcAspect < outAspect)) {
            float scale = outAspect / srcAspect;
            float vPad = 1.0 - scale;
            if (fitMode == 0)
                srcUV = vec2(outUV.x, clamp((outUV.y - vPad*0.5)/(1.0-vPad),0.0,1.0));
            else
                srcUV = vec2(outUV.x, clamp((outUV.y - 0.5*(1.0 - scale))/scale,0.0,1.0));
        } else {
            float scale = srcAspect / outAspect;
            float hPad = 1.0 - scale;
            if (fitMode == 0)
                srcUV = vec2(clamp((outUV.x - hPad*0.5)/(1.0-hPad),0.0,1.0), outUV.y);
            else
                srcUV = vec2(clamp((outUV.x - 0.5*(1.0 - scale))/scale,0.0,1.0), outUV.y);
        }
    } else {
        vec2 p0 = corner0;
        vec2 p1 = corner1;
        vec2 p2 = corner2;
        vec2 p3 = corner3;
        srcUV = solveUV(outUV, p0, p1, p2, p3);
    }

    vec2 bleedNorm = edgeBleed / inSize;
    vec2 sampUV = clamp(srcUV, -bleedNorm, vec2(1.0) + bleedNorm);

    vec4 color = IMG_NORM_PIXEL(inputImage, sampUV);
    color.rgb = pow(color.rgb, vec3(1.0 / gamma));

    gl_FragColor = color;
}
