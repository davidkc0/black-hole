Shader "Custom/BlackHoleHalo"
{
    Properties
    {
        _MainTex ("Background Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _BlackHolePosition ("Black Hole Position", Vector) = (0.5, 0.5, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // Define constants.
            static const float distortionStrength = 1.0;
            static const float maxLensingAngle = 28.3;
            static const float blackRadius = 0.15;
            static const float edgeBrightnessFactor = 2.04;
            
            // Define colors.
            static const float3 brightColor1 = float3(0.95, 0.87, 0.75);
            static const float3 brightColor2 = float3(0.966, 0.91, 0.84);
            static const float3 accretionDiskFadeColor = float3(1.0, 0.67, 0.2);
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            
            float4 _BlackHolePosition;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            // Declare the aspectRatioCorrectionFactor and sourcePosition variables
            float2 aspectRatioCorrectionFactor;
            float2 sourcePosition;
            
            // Utility from HLSL. Clamps an input to a 0-1 range.
            float saturate(float x)
            {
                return clamp(x, 0.0, 1.0);
            }
            
            // Inverse of the lerp/mix function. Useful for getting back the 0-1 interpolant from a given input range.
            // The resulting interpolant cannot extend beyond the 0-1 range, even if the inputs are outside of their expected provided bounds.
            float inverseLerp(float from, float to, float x)
            {
                return saturate((x - from) / (to - from));
            }
            
            float2 vectorNoise(float2 coords)
            {
                // Sample from the noise texture and interpret the results as an angle.
                float angle = tex2D(_NoiseTex, coords).x * 16.03;
                
                // Convert the aforementioned angle into a 0-1 range vector.
                return float2(cos(angle), sin(angle)) * 0.5 + 0.5;
            }
            
            // Rotates a vector by a given angle.
            // This is based on the Z rotation matrix, but simplified.
            float2 rotatedBy(float2 v, float theta)
            {
                float s = sin(theta);
                float c = cos(theta);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }
            
            float calculateGravitationalLensingAngle(float2 uv)
            {
                // Calculate how far the given pixel is from the source of the distortion. This autocorrects for the aspect ratio resulting in
                // non-square calculations.
                float distanceToSource = max(distance((uv - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition), 0.0);
                
                // Calculate the lensing angle based on the aforementioned distance. This uses distance-based exponential decay to ensure that the effect
                // does not extend far past the source itself.
                return distortionStrength * maxLensingAngle * exp(-distanceToSource / blackRadius * 2.0);
            }
            
            float4 applyColorEffects(float4 color, float gravitationalLensingAngle, float2 uv, float2 distortedUV)
            {
                // Calculate offset values based on noise.
                float2 uvOffset1 = vectorNoise(distortedUV + float2(0, _Time.y * 0.8));
                float2 uvOffset2 = vectorNoise(distortedUV * 0.4 + float2(0, _Time.y * 0.7));
                
                // Calculate color interpolants. These are used below.
                // The black hole uses a little bit of the UV offset noise for calculating the edge boundaries. This helps make the effect feel a bit less
                // mathematically perfect and more aesthetically interesting.
                float offsetDistanceToSource = max(distance((uv - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition + uvOffset1 * 0.004), 0.0);
                float blackInterpolant = inverseLerp(blackRadius, blackRadius * 0.85, offsetDistanceToSource);
                float brightInterpolant = pow(inverseLerp(blackRadius * (1.01 + uvOffset2.x * 0.1), blackRadius * 0.97, offsetDistanceToSource), 1.6) * 0.6 + gravitationalLensingAngle * 7.777 / maxLensingAngle;
                float accretionDiskInterpolant = inverseLerp(blackRadius * 1.93, blackRadius * 1.3, offsetDistanceToSource) * (1.0 - brightInterpolant);
                
                // Calculate the inner bright color. This is the color used right at the edge of the black hole itself, where everything is burning due to extraordinary amounts of particle friction.
                float4 brightColor = float4(lerp(brightColor1, brightColor2, uvOffset1.y), 1) * edgeBrightnessFactor;
                
                // Interpolate towards the bright color first.
                color = lerp(color, brightColor, saturate(brightInterpolant) * distortionStrength);
                
                // Interpolate towards the accretion disk's color next. This is what is drawn a bit beyond the burning bright edge. It is still heated, but not as much, and as such is closer to an orange
                // glow than a blazing yellowish white.
                color = lerp(color, float4(accretionDiskFadeColor, 1), accretionDiskInterpolant * distortionStrength);
                
                // Lastly, place the black hole in the center above everything.
                color = lerp(color, float4(0, 0, 0, 1), blackInterpolant * distortionStrength);
                
                return color;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate dynamic global values.
                aspectRatioCorrectionFactor = float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                sourcePosition = _BlackHolePosition.xy;
                
                // Calculate the gravitational lensing angle and the coordinates that result from following its rotation.
                float gravitationalLensingAngle = calculateGravitationalLensingAngle(i.uv);
                float2 distortedUV = rotatedBy(i.uv - 0.5, gravitationalLensingAngle) + 0.5;
                
                // Sample the background texture using the distorted UV.
                fixed4 backgroundColor = tex2D(_MainTex, distortedUV);
                
                // Calculate the colors based on the above information, and supply them to the output color.
                return applyColorEffects(backgroundColor, gravitationalLensingAngle, i.uv, distortedUV);
            }
            ENDCG
        }
    }
}