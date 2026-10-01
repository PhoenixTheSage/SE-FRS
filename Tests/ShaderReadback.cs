using System.Runtime.InteropServices;
using ClientPlugin.Frs;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device=SharpDX.Direct3D11.Device;

// Isolated hardware context. Exercises the production helper and baked DXBC;
// it neither attaches to nor modifies a running Space Engineers session.
public static class ShaderReadback
{
    public static void Main()
    {
        using var device=new Device(DriverType.Hardware,DeviceCreationFlags.None,FeatureLevel.Level_11_0);
        using var dxgi=device.QueryInterface<SharpDX.DXGI.Device>();
        using var adapter=dxgi.Adapter;
        Console.WriteLine("GPU: "+adapter.Description.Description);
        var context=device.ImmediateContext;
        const int width=32,height=16;
        var description=new Texture2DDescription
        {
            Width=width,Height=height,ArraySize=1,MipLevels=1,
            Format=Format.R32_Float,SampleDescription=new SampleDescription(1,0),
            Usage=ResourceUsage.Default,BindFlags=BindFlags.ShaderResource|BindFlags.RenderTarget
        };
        using var depth=new Texture2D(device,description);
        using var depthRtv=new RenderTargetView(device,depth);
        context.ClearRenderTargetView(depthRtv,new RawColor4(.5f,0,0,0));
        var identity=new float[]{1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1};
        try
        {
            foreach(var move in new[]{(x:0f,y:0f),(x:.125f,y:0f),(x:0f,y:.25f),(x:.125f,y:.25f)})
            {
                var previous=(float[])identity.Clone();previous[12]=move.x;previous[13]=move.y;
                context.ClearState();
                var pointer=FrsD3d.GenerateCameraMotionVectors(device,context,depth,width,height,identity,identity,previous);
                if(pointer==IntPtr.Zero)throw new Exception(FrsNative.LastError);
                Marshal.AddRef(pointer);
                using var motion=new Texture2D(pointer);
                var stageDesc=motion.Description;stageDesc.BindFlags=BindFlags.None;stageDesc.Usage=ResourceUsage.Staging;stageDesc.CpuAccessFlags=CpuAccessFlags.Read;
                using var stage=new Texture2D(device,stageDesc);
                context.CopyResource(motion,stage);
                var read=context.MapSubresource(stage,0,MapMode.Read,SharpDX.Direct3D11.MapFlags.None);
                try
                {
                    for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                    {
                        var at=IntPtr.Add(read.DataPointer,y*read.RowPitch+x*4);
                        float actualX=(float)BitConverter.UInt16BitsToHalf(unchecked((ushort)Marshal.ReadInt16(at)));
                        float actualY=(float)BitConverter.UInt16BitsToHalf(unchecked((ushort)Marshal.ReadInt16(at,2)));
                        float expectedX=move.x*width*.5f,expectedY=-move.y*height*.5f;
                        if(!float.IsFinite(actualX)||!float.IsFinite(actualY)||Math.Abs(actualX-expectedX)>.01f||Math.Abs(actualY-expectedY)>.01f)
                            throw new Exception($"MV readback at {x},{y}: ({actualX},{actualY}), expected ({expectedX},{expectedY})");
                    }
                }
                finally { context.UnmapSubresource(stage,0); }
                Console.WriteLine($"PASS 512 pixels: current-to-previous MV=({move.x*width*.5f},{-move.y*height*.5f})");
            }
            Console.WriteLine("All 2,048 baked-shader motion samples passed. Full in-game acceptance remains manual.");
        }
        finally { context.ClearState();FrsD3d.Release(); }
    }
}
namespace ClientPlugin.Frs { public static class DebugLog { public static void Write(string value) { } } }
