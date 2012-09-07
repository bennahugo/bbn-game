/// ----------------------------------------------
/// @author - Brandon James Talbot
///
/// This effect holds the direction of the screen so that the hudBoxes are drawn correctly facing the player
///
/// Referance - spaceShooter sample code from MSN Microsoft
///
///-----------------------------------------------

struct VS_INPUT
{
	float4 Pos: POSITION;
	float4 colour: COLOR;
};

struct VS_OUTPUT 
{
   float4 Pos:   POSITION;
   float4 colour: COLOR;
};

float2 viewPort;

VS_OUTPUT SimpleScreenVS(VS_INPUT In)
{
   VS_OUTPUT Out;

	// Move to screen space. Vertices passed in are already transformed...
	Out.Pos.x = (In.Pos.x - (viewPort.x / 2)) / (viewPort.x / 2);
	Out.Pos.y = (In.Pos.y - (viewPort.y / 2)) / (viewPort.y / 2);
	Out.Pos.z = 0;
	Out.Pos.w = 1;

	Out.colour = In.colour;
	
    return Out;
}

float4 SimpleScreenPS(float4 color : COLOR0) : COLOR0
{
    return color;
}

technique SimpleScreen
{
   pass Single_Pass
   {
        CULLMODE = NONE;
        ALPHABLENDENABLE = TRUE;
        SRCBLEND = SRCALPHA;
        DESTBLEND = INVSRCALPHA;
        ZENABLE = FALSE; //Always want this on top
        POINTSIZE = 1;
      
		VertexShader = compile vs_1_1 SimpleScreenVS();
		PixelShader = compile ps_1_1 SimpleScreenPS();
   }
}
