using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

namespace VRoidXYTool.MMD
{
    //注意！VMDファイルではShiftJISが用いられているが、UnityでShiftJISを使うには一工夫必要！！
    //Unity ShiftJISで検索すること

    public class VMDReader
    {
        //人ボーンのキーフレームのボーンごとの集合
        public class BoneKeyFrameGroup
        {
            readonly Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

            public enum BoneNames
            {
                全ての親, センター, グルーブ, 左足ＩＫ, 左つま先ＩＫ, 右足ＩＫ, 右つま先ＩＫ, 下半身, 上半身, 上半身2,
                首, 頭, 左目, 右目, 両目, 左肩, 左腕, 左腕捩れ, 左ひじ, 左手首,
                右肩, 右腕, 右腕捩れ, 右ひじ, 右手首, 左足, 左ひざ, 左足首, 左つま先, 右足,
                右ひざ, 右足首, 右つま先, 左親指１, 左親指２, 左人指１, 左人指２, 左人指３, 左中指１, 左中指２,
                左中指３, 左薬指１, 左薬指２, 左薬指３, 左小指１, 左小指２, 左小指３, 右親指１, 右親指２, 右人指１,
                右人指２, 右人指３, 右中指１, 右中指２, 右中指３, 右薬指１, 右薬指２, 右薬指３, 右小指１, 右小指２,
                右小指３, None
            }

            public static List<string> StringBoneNames
            {
                get
                {
                    if (boneStringNames == null)
                    {
                        boneStringNames = Enum.GetNames(typeof(BoneNames)).ToList();
                        boneStringNames.Remove(BoneNames.None.ToString());
                    }

                    return boneStringNames;
                }

                set { }
            }
            private static List<string> boneStringNames;

            public BoneNames Name { get; private set; }

            //全てのBoneKeyFrames
            public List<VMD.BoneKeyFrame> BoneKeyFrames = new List<VMD.BoneKeyFrame>();
            //Positionデータを保持したBoneKeyFramesのリスト
            public List<VMD.BoneKeyFrame> BonePositionKeyFrames = new List<VMD.BoneKeyFrame>();
            //Rotationデータを保持したBoneKeyFramesのリスト
            public List<VMD.BoneKeyFrame> BoneRotationKeyFrames = new List<VMD.BoneKeyFrame>();

            int frameNumberCash = 0;

            public VMD.BoneKeyFrame CurrentKeyFrame { get; private set; }
            public VMD.BoneKeyFrame.Interpolation Interpolation { get; private set; }
            public VMD.BoneKeyFrame NextKeyFrame { get; private set; }
            public VMD.BoneKeyFrame LastPositionKeyFrame { get; private set; }
            public VMD.BoneKeyFrame LastRotationKeyFrame { get; private set; }
            public VMD.BoneKeyFrame NextPositionKeyFrame { get; private set; }
            public VMD.BoneKeyFrame NextRotationKeyFrame { get; private set; }

            public BoneKeyFrameGroup(BoneNames name)
            {
                Name = name;
            }

            public VMD.BoneKeyFrame GetKeyFrame(int frameNumber)
            {
                if (frameNumber == frameNumberCash + 1)
                {
                    frameNumberCash = frameNumber;
                    return GetKeyFrameUsingCash(frameNumber);
                }

                frameNumberCash = frameNumber;
                return GetKeyFrameWithoutCash(frameNumber);
            }
            private VMD.BoneKeyFrame GetKeyFrameUsingCash(int frameNumber)
            {
                CurrentKeyFrame = BoneKeyFrames.Find(x => x.FrameNumber == frameNumber);

                if (CurrentKeyFrame == null && (NextPositionKeyFrame != null || NextRotationKeyFrame != null))
                {
                    return null;
                }

                if (CurrentKeyFrame != null && CurrentKeyFrame.Position != Vector3.zero)
                {
                    LastPositionKeyFrame = CurrentKeyFrame;
                    NextPositionKeyFrame = BonePositionKeyFrames.Find(x => x.FrameNumber > frameNumber);
                }
                if (CurrentKeyFrame != null && CurrentKeyFrame.Rotation != ZeroQuaternion)
                {
                    LastRotationKeyFrame = CurrentKeyFrame;
                    NextRotationKeyFrame = BoneRotationKeyFrames.Find(x => x.FrameNumber > frameNumber);
                }

                if (NextPositionKeyFrame == null && NextRotationKeyFrame == null)
                {
                    NextKeyFrame = null;
                }
                else if (NextPositionKeyFrame != null && NextRotationKeyFrame != null)
                {
                    NextKeyFrame = NextRotationKeyFrame.FrameNumber < NextPositionKeyFrame.FrameNumber ? NextRotationKeyFrame : NextPositionKeyFrame;
                }
                else if (NextPositionKeyFrame == null)
                {
                    NextKeyFrame = NextRotationKeyFrame;
                }
                else if (NextRotationKeyFrame == null)
                {
                    NextKeyFrame = NextPositionKeyFrame;
                }

                if (NextKeyFrame != null)
                {
                    Interpolation = NextKeyFrame.BoneInterpolation;
                }
                else
                {
                    Interpolation = null;
                }

                return CurrentKeyFrame;
            }
            private VMD.BoneKeyFrame GetKeyFrameWithoutCash(int frameNumber)
            {
                CurrentKeyFrame = BoneKeyFrames.Find(x => x.FrameNumber == frameNumber);

                LastPositionKeyFrame = BonePositionKeyFrames.FindLast(x => x.FrameNumber <= frameNumber);
                LastRotationKeyFrame = BoneRotationKeyFrames.FindLast(x => x.FrameNumber <= frameNumber);
                NextPositionKeyFrame = BonePositionKeyFrames.Find(x => x.FrameNumber > frameNumber);
                NextRotationKeyFrame = BoneRotationKeyFrames.Find(x => x.FrameNumber > frameNumber);

                if (NextPositionKeyFrame == null && NextRotationKeyFrame == null)
                {
                    NextKeyFrame = null;
                }
                else if (NextPositionKeyFrame != null && NextRotationKeyFrame != null)
                {
                    NextKeyFrame = NextRotationKeyFrame.FrameNumber < NextPositionKeyFrame.FrameNumber ? NextRotationKeyFrame : NextPositionKeyFrame;
                }
                else if (NextPositionKeyFrame == null)
                {
                    NextKeyFrame = NextRotationKeyFrame;
                }
                else if (NextRotationKeyFrame == null)
                {
                    NextKeyFrame = NextPositionKeyFrame;
                }

                if (NextKeyFrame != null)
                {
                    Interpolation = NextKeyFrame.BoneInterpolation;
                }
                else
                {
                    Interpolation = null;
                }

                return CurrentKeyFrame;
            }

            public void AddKeyFrame(VMD.BoneKeyFrame vmdBoneFrame)
            {
                BoneKeyFrames.Add(vmdBoneFrame);
                if (vmdBoneFrame.Position != Vector3.zero) { BonePositionKeyFrames.Add(vmdBoneFrame); }
                if (vmdBoneFrame.Rotation != ZeroQuaternion) { BoneRotationKeyFrames.Add(vmdBoneFrame); }
            }

            public void OrderByFrame()
            {
                BoneKeyFrames = BoneKeyFrames.OrderBy(x => x.FrameNumber).ToList();
                BonePositionKeyFrames = BonePositionKeyFrames.OrderBy(x => x.FrameNumber).ToList();
                BoneRotationKeyFrames = BoneRotationKeyFrames.OrderBy(x => x.FrameNumber).ToList();
            }
        }

        public class FaceKeyFrameGroup
        {
            public string MorphName { get; private set; }
            public List<VMD.FaceKeyFrame> FaceKeyFrames { get; private set; } = new List<VMD.FaceKeyFrame>();

            int frameNumberCash = 0;

            VMD.FaceKeyFrame CurrentMorphKeyFrame = null;
            public VMD.FaceKeyFrame LastMorphKeyFrame = null;
            public VMD.FaceKeyFrame NextMorphKeyFrame = null;

            public FaceKeyFrameGroup(string morphName)
            {
                MorphName = morphName;
            }

            public VMD.FaceKeyFrame GetKeyFrame(int frameNumber)
            {
                if (frameNumber == frameNumberCash + 1)
                {
                    frameNumberCash = frameNumber;
                    return GetKeyFrameUsingCash(frameNumber);
                }

                frameNumberCash = frameNumber;
                return GetKeyFrameWithoutCash(frameNumber);
            }
            VMD.FaceKeyFrame GetKeyFrameUsingCash(int frameNumber)
            {
                if (NextMorphKeyFrame == null)
                {
                    return null;
                }

                if (frameNumber == NextMorphKeyFrame.FrameNumber)
                {
                    LastMorphKeyFrame = NextMorphKeyFrame;
                    CurrentMorphKeyFrame = NextMorphKeyFrame;
                    NextMorphKeyFrame = FaceKeyFrames.Find(x => x.FrameNumber > frameNumber);
                    return CurrentMorphKeyFrame;
                }

                CurrentMorphKeyFrame = null;
                return CurrentMorphKeyFrame;
            }
            VMD.FaceKeyFrame GetKeyFrameWithoutCash(int frameNumber)
            {
                CurrentMorphKeyFrame = FaceKeyFrames.FindLast(x => x.FrameNumber == frameNumber);
                LastMorphKeyFrame = FaceKeyFrames.FindLast(x => x.FrameNumber < frameNumber);
                NextMorphKeyFrame = FaceKeyFrames.Find(x => x.FrameNumber > frameNumber);

                return CurrentMorphKeyFrame;
            }
        }

        public VMD RawVMD { get; private set; }

        public int FrameCount { get; private set; } = -1;

        //ボーンごとに分けたキーフレームの集合をボーンの番号順にリストに入れる、これはコンストラクタで初期化される
        public List<BoneKeyFrameGroup> BoneKeyFrameGroups = new List<BoneKeyFrameGroup>();

        //表情ごとに分けたキーフレームの集合を表情の番号順にリストに入れる、これはコンストラクタで初期化される
        public Dictionary<string, FaceKeyFrameGroup> FaceKeyFrameGroups = new Dictionary<string, FaceKeyFrameGroup>();

        void InitializeBoneKeyFrameGroups()
        {
            BoneKeyFrameGroups.Clear();
            for (int i = 0; i < BoneKeyFrameGroup.StringBoneNames.Count; i++)
            {
                if ((BoneKeyFrameGroup.BoneNames)i == BoneKeyFrameGroup.BoneNames.None) { continue; }
                BoneKeyFrameGroups.Add(new BoneKeyFrameGroup((BoneKeyFrameGroup.BoneNames)i));
            }
        }
        public VMDReader()
        {
            InitializeBoneKeyFrameGroups();
            RawVMD = new VMD();
        }
        public VMDReader(string filePath)
        {
            InitializeBoneKeyFrameGroups();
            ReadVMD(filePath);
        }

        public void ReadVMD(string filePath)
        {
            Console.WriteLine($"开始读取{filePath}");
            RawVMD = new VMD(filePath);
            Console.WriteLine($"读取完毕{filePath}");
            //人ボーンのキーフレームをグループごとに分けてBoneKeyFrameGroupsに入れる
            foreach (VMD.BoneKeyFrame boneKeyFrame in RawVMD.BoneKeyFrames)
            {
                if (!BoneKeyFrameGroup.StringBoneNames.Contains(boneKeyFrame.Name)) { continue; }
                BoneKeyFrameGroups[BoneKeyFrameGroup.StringBoneNames.IndexOf(boneKeyFrame.Name)].AddKeyFrame(boneKeyFrame);
            }
            //いちおうフレームごとに並べておく
            BoneKeyFrameGroups.ForEach(x => x.OrderByFrame());
            //人ボーンのフレームが見当たらなかったらこれ以上しない
            if (BoneKeyFrameGroups.All(x => x.BoneKeyFrames.Count == 0)) { return; }

            //ついでに最終フレームも求めておく
            FrameCount = BoneKeyFrameGroups.Where(x => x.BoneKeyFrames.Count > 0).Max(x => x.BoneKeyFrames.Last().FrameNumber);
            Console.WriteLine($"FrameCount:{FrameCount}");

            //全表情ボーンをフレームごとに並び替えておく
            RawVMD.FaceKeyFrames.OrderBy(x => x.FrameNumber);
            //表情ボーンのキーフレームをグループごとに分けてFaceKeyFrameGroupsに入れる
            foreach (VMD.FaceKeyFrame faceKeyFrame in RawVMD.FaceKeyFrames)
            {
                string morphName = faceKeyFrame.MorphName;
                if (morphName == null) { continue; }
                if (!FaceKeyFrameGroups.Keys.Contains(morphName))
                {
                    FaceKeyFrameGroups.Add(morphName, new FaceKeyFrameGroup(faceKeyFrame.MorphName));
                }
                FaceKeyFrameGroups[morphName].FaceKeyFrames.Add(faceKeyFrame);
            }
        }

        public static async Task<VMDReader> ReadVMDAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                VMDReader vmdReader = new VMDReader(filePath);
                return vmdReader;
            });
        }

        public BoneKeyFrameGroup GetBoneKeyFrameGroup(BoneKeyFrameGroup.BoneNames boneName)
        {
            return BoneKeyFrameGroups[(int)boneName];
        }

        //普通にnullも返ってくる
        public VMD.BoneKeyFrame GetBoneKeyFrame(BoneKeyFrameGroup.BoneNames boneName, int frameNumber)
        {
            return BoneKeyFrameGroups[(int)boneName].GetKeyFrame(frameNumber);
        }

        public static Vector3 InterporatePosition(VMDReader vmdReader, BoneKeyFrameGroup.BoneNames boneName, int frameNumber)
        {
            VMDReader.BoneKeyFrameGroup vmdBoneFrameGroup = vmdReader.GetBoneKeyFrameGroup(boneName);
            VMD.BoneKeyFrame lastFrame = vmdBoneFrameGroup.LastPositionKeyFrame;
            VMD.BoneKeyFrame nextFrame = vmdBoneFrameGroup.NextPositionKeyFrame;

            if (lastFrame != null && nextFrame != null)
            {
                float xInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, lastFrame.FrameNumber, nextFrame.FrameNumber);
                float yInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, lastFrame.FrameNumber, nextFrame.FrameNumber);
                float zInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, lastFrame.FrameNumber, nextFrame.FrameNumber);

                float xInterpolation = Mathf.Lerp(lastFrame.Position.x, nextFrame.Position.x, xInterpolationRate);
                float yInterpolation = Mathf.Lerp(lastFrame.Position.y, nextFrame.Position.y, yInterpolationRate);
                float zInterpolation = Mathf.Lerp(lastFrame.Position.z, nextFrame.Position.z, zInterpolationRate);
                return new Vector3(xInterpolation, yInterpolation, zInterpolation);
            }
            else if (lastFrame == null && nextFrame != null)
            {
                float xInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, 0, nextFrame.FrameNumber);
                float yInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, 0, nextFrame.FrameNumber);
                float zInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, 0, nextFrame.FrameNumber);

                float xInterpolation = Mathf.Lerp(0, nextFrame.Position.x, xInterpolationRate);
                float yInterpolation = Mathf.Lerp(0, nextFrame.Position.y, yInterpolationRate);
                float zInterpolation = Mathf.Lerp(0, nextFrame.Position.z, zInterpolationRate);
                return new Vector3(xInterpolation, yInterpolation, zInterpolation);
            }
            else if (nextFrame == null && lastFrame != null)
            {
                return lastFrame.Position;
            }

            return Vector3.zero;
        }

        public static Quaternion InterporateRotation(VMDReader vmdReader, BoneKeyFrameGroup.BoneNames boneName, int frameNumber)
        {
            VMDReader.BoneKeyFrameGroup vmdBoneFrameGroup = vmdReader.GetBoneKeyFrameGroup(boneName);
            VMD.BoneKeyFrame lastFrame = vmdBoneFrameGroup.LastRotationKeyFrame;
            VMD.BoneKeyFrame nextFrame = vmdBoneFrameGroup.NextRotationKeyFrame;

            if (lastFrame != null && nextFrame != null)
            {
                float rotationInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Rotation, frameNumber, lastFrame.FrameNumber, nextFrame.FrameNumber);

                return Quaternion.identity.PlusRotation(Quaternion.Slerp(lastFrame.Rotation, nextFrame.Rotation, rotationInterpolationRate));
            }
            else if (lastFrame == null && nextFrame != null)
            {
                float rotationInterpolationRate = vmdBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Rotation, frameNumber, 0, nextFrame.FrameNumber);
                return Quaternion.identity.PlusRotation(Quaternion.Slerp(Quaternion.identity, nextFrame.Rotation, rotationInterpolationRate));
            }
            else if (lastFrame != null && nextFrame == null)
            {
                return Quaternion.identity.PlusRotation(lastFrame.Rotation);
            }

            return Quaternion.identity;
        }
    }

    public class VMD
    {
        //人ボーンのキーフレーム
        public class BoneKeyFrame
        {
            //ボーン名
            public string Name { get; private set; } = "";

            //フレーム番号
            public int FrameNumber { get; private set; }

            //ボーンの位置、UnityでいうlocalPosition、ただし縮尺はUnityの空間の約10倍
            public Vector3 Position { get; private set; }

            //ボーンの回転、UnityでいうlocalRotation
            public Quaternion Rotation { get; private set; }

            //3次ベジェ曲線での補間
            public class Interpolation
            {
                public class BezierCurvePoint
                {
                    internal byte X = new byte();
                    internal byte Y = new byte();
                }

                internal BezierCurvePoint[] X = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Y = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Z = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Rotation = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };

                //コンストラクタにてX,Y,Z,Rotationを入れる
                List<BezierCurvePoint[]> BezierCurves;
                public enum BezierCurveNames { X, Y, Z, Rotation }

                public Interpolation()
                {
                    BezierCurves = new List<BezierCurvePoint[]>() { X, Y, Z, Rotation };
                }

                //P0,P1,P2,P3を通る3次のもので、
                //P0 = (0,0)と P3 = (1,1)かつ、0 < x < 1で単調増加となるようなベジェ曲線が用いられている。
                //P1とP2がVMDファイルから得られるので理論上曲線が求まるが、下では媒介変数表示と2分法を用いて値を近似している。
                //edvakfさんのコードを参考にしました。
                public float GetInterpolationValue(BezierCurveNames bazierCurveName, int currentFrame, int beginFrame, int endFrame)
                {
                    BezierCurvePoint[] bezierCurve = BezierCurves[(int)bazierCurveName];

                    float x = (float)(currentFrame - beginFrame) / (float)(endFrame - beginFrame);

                    float t = 0.5f;
                    float s = 0.5f;
                    for (int i = 0; i < 15; i++)
                    {
                        //実は保存されているときには127*127である。それを比に落とし込む。
                        float zero = (3 * s * s * t * bezierCurve[0].X / 127) + (3 * s * t * t * bezierCurve[1].X / 127) + (t * t * t) - x;

                        if (Mathf.Abs(zero) < 0.00001f) { break; }

                        if (zero > 0) { t -= 1 / (4 * Mathf.Pow(2, i)); }
                        else { t += 1 / (4 * Mathf.Pow(2, i)); }

                        s = 1 - t;
                    }

                    //実は保存されているときには127*127である。それを比に落とし込む。
                    return (3 * s * s * t * bezierCurve[0].Y / 127) + (3 * s * t * t * bezierCurve[1].Y / 127) + (t * t * t);
                }
            }

            public Interpolation BoneInterpolation = new Interpolation();

            public BoneKeyFrame() { }
            public BoneKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                byte[] nameBytes = binaryReader.ReadBytes(15);
                
                Name = ToEncoding.ToUnicode(nameBytes);
                //ヌル文字除去
                Name = Name.TrimEnd('\0').TrimEnd('?').TrimEnd('\0');
                FrameNumber = binaryReader.ReadInt32();
                //座標系の違いにより、x,zをマイナスにすることに注意
                Position = Util.ReadVector3(binaryReader);
                //座標系の違いにより、x,zをマイナスにすることに注意
                Rotation = Util.ReadQuaternion(binaryReader);

                //VMDでは3次ベジェ曲線において
                //X軸の補間 パラメータP1(X_x1, X_y1),P2(X_x2, X_y2)
                //Y軸の補間 パラメータP1(Y_x1, Y_y1),P2(Y_x2, Y_y2)
                //Z軸の補間 パラメータP1(Z_x1, Z_y1),P2(Z_x2, Z_y2)
                //回転の補間パラメータP1(R_x1, R_y1),P2(R_x2, R_y2)
                //としたとき、インデックスでいうと0番目から4,8番目と4の倍数のところに
                //X_x1, X_y1, X_x2, X_y2, Y_x1, Y_y1, ...と順番に入っている
                //また、X_x1などの値はすべて1byteである

                void parseInterpolation(Interpolation.BezierCurvePoint[] x)
                {
                    x[0].X = binaryReader.ReadByte();
                    binaryReader.ReadBytes(3);
                    x[0].Y = binaryReader.ReadByte();
                    binaryReader.ReadBytes(3);
                    x[1].X = binaryReader.ReadByte();
                    binaryReader.ReadBytes(3);
                    x[1].Y = binaryReader.ReadByte();
                    binaryReader.ReadBytes(3);
                }

                parseInterpolation(BoneInterpolation.X);
                parseInterpolation(BoneInterpolation.Y);
                parseInterpolation(BoneInterpolation.Z);
                parseInterpolation(BoneInterpolation.Rotation);
            }
        };

        //表情のキーフレーム
        public class FaceKeyFrame
        {
            //表情モーフ名
            public string MorphName { get; private set; }
            //表情モーフのウェイト
            public float Weight { get; private set; }
            //フレーム番号
            public uint FrameNumber { get; private set; }

            public FaceKeyFrame() { }
            public FaceKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                byte[] nameBytes = binaryReader.ReadBytes(15);
                MorphName = ToEncoding.ToUnicode(nameBytes);
                //ヌル文字除去
                MorphName = MorphName.TrimEnd('\0').TrimEnd('?').TrimEnd('\0');
                FrameNumber = binaryReader.ReadUInt32();
                Weight = binaryReader.ReadSingle();
            }
        };

        //カメラのキーフレーム
        public class CameraKeyFrame
        {
            //フレーム番号
            public int Frame { get; private set; }
            //目標点とカメラの距離(目標点がカメラ前面でマイナス)
            public float Distance { get; private set; }
            //目標点の位置
            public Vector3 Position { get; private set; }
            //カメラの回転
            public Quaternion Rotation { get; private set; }
            //補間曲線
            public Interpolation CameraInterpolation = new Interpolation();
            //public byte[][] InterPolation { get; private set; } = new byte[6][];
            //視野角
            public float Angle;
            //おそらくパースペクティブかどうか0or1、0でパースペクティブ
            public bool Perspective;

            //3次ベジェ曲線での補間
            public class Interpolation
            {
                public class BezierCurvePoint
                {
                    internal byte X = new byte();
                    internal byte Y = new byte();
                }

                internal BezierCurvePoint[] X = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Y = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Z = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Rotation = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Distance = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };
                internal BezierCurvePoint[] Angle = new BezierCurvePoint[] { new BezierCurvePoint(), new BezierCurvePoint() };

                //コンストラクタにてX,Y,Z,Rotationを入れる
                List<BezierCurvePoint[]> BezierCurves;
                public enum BezierCurveNames { X, Y, Z, Rotation, Distance, Angle }

                public Interpolation()
                {
                    BezierCurves = new List<BezierCurvePoint[]>() { X, Y, Z, Rotation, Distance, Angle };
                }

                //P0,P1,P2,P3を通る3次のもので、
                //P0 = (0,0)と P3 = (1,1)かつ、0 < x < 1で単調増加となるようなベジェ曲線が用いられている。
                //P1とP2がVMDファイルから得られるので理論上曲線が求まるが、下では媒介変数表示と2分法を用いて値を近似している。
                //edvakfさんのコードを参考にしました。
                public float GetInterpolationValue(BezierCurveNames bazierCurveName, int currentFrame, int beginFrame, int endFrame)
                {
                    BezierCurvePoint[] bezierCurve = BezierCurves[(int)bazierCurveName];

                    float x = (float)(currentFrame - beginFrame) / (float)(endFrame - beginFrame);

                    float t = 0.5f;
                    float s = 0.5f;
                    for (int i = 0; i < 15; i++)
                    {
                        //実は保存されているときには127*127である。それを比に落とし込む。
                        float zero = (3 * s * s * t * bezierCurve[0].X / 127) + (3 * s * t * t * bezierCurve[1].X / 127) + (t * t * t) - x;

                        if (Mathf.Abs(zero) < 0.00001f) { break; }

                        if (zero > 0) { t -= 1 / (4 * Mathf.Pow(2, i)); }
                        else { t += 1 / (4 * Mathf.Pow(2, i)); }

                        s = 1 - t;
                    }

                    //実は保存されているときには127*127である。それを比に落とし込む。
                    return (3 * s * s * t * bezierCurve[0].Y / 127) + (3 * s * t * t * bezierCurve[1].Y / 127) + (t * t * t);
                }
            }

            public CameraKeyFrame() { }
            public CameraKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                Frame = binaryReader.ReadInt32();

                //目標点とカメラの距離(目標点がカメラ前面でマイナス)
                Distance = binaryReader.ReadInt32();

                //座標系、x,zをマイナスにすることに注意
                Position = Util.ReadVector3(binaryReader);

                //座標系、x,zをマイナスにすることに注意
                Rotation = Util.ReadQuaternion(binaryReader);

                void parseInterpolation(Interpolation.BezierCurvePoint[] x)
                {
                    x[0].X = binaryReader.ReadByte();
                    x[0].Y = binaryReader.ReadByte();
                    x[1].X = binaryReader.ReadByte();
                    x[1].Y = binaryReader.ReadByte();
                }

                parseInterpolation(CameraInterpolation.X);
                parseInterpolation(CameraInterpolation.Y);
                parseInterpolation(CameraInterpolation.Z);
                parseInterpolation(CameraInterpolation.Rotation);
                parseInterpolation(CameraInterpolation.Distance);
                parseInterpolation(CameraInterpolation.Angle);

                Angle = binaryReader.ReadSingle();
                Perspective = BitConverter.ToBoolean(binaryReader.ReadBytes(3), 0);
            }
        };

        //照明のキーフレーム
        public class LightKeyFrame
        {
            //フレーム番号
            public int Frame { get; private set; }
            //ライトの色、R,G,Bの順に格納されている、0から1
            public float[] LightColor { get; private set; } = new float[3];
            //ライトの位置
            public Vector3 Position { get; private set; }

            public LightKeyFrame() { }
            public LightKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                Frame = binaryReader.ReadInt32();

                //R,G,Bの順に格納されている、0から1
                float[] LightColor = (from n in Enumerable.Range(0, 3) select binaryReader.ReadSingle()).ToArray();

                //座標系の違いによりx,zをマイナスとする
                Position = Util.ReadVector3(binaryReader);
            }
        };

        //セルフ影のキーフレーム
        public class SelfShadowKeyFrame
        {
            public int Frame { get; private set; }
            //セルフシャドウの種類
            public byte Type { get; private set; }
            //セルフシャドウの距離
            public float Distance { get; private set; }

            public SelfShadowKeyFrame() { }
            public SelfShadowKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                Frame = binaryReader.ReadInt32();
                Type = binaryReader.ReadByte();
                Distance = binaryReader.ReadSingle();
            }
        }

        //IKのキーフレーム
        public class IKKeyFrame
        {
            //IKの名前とそのIKが有効かどうか
            public class VMDIKEnable
            {
                public string IKName;
                public bool Enable;
            };

            public int Frame { get; private set; }
            //
            public bool Display { get; private set; }
            public List<VMDIKEnable> IKEnable { get; private set; } = new List<VMDIKEnable>();

            public IKKeyFrame() { }
            public IKKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

            public void Read(BinaryReader binaryReader)
            {
                byte[] buffer = new byte[20];
                Frame = binaryReader.ReadInt32();
                Display = BitConverter.ToBoolean(new byte[] { binaryReader.ReadByte() }, 0);
                int ikCount = binaryReader.ReadInt32();
                for (int i = 0; i < ikCount; i++)
                {
                    binaryReader.Read(buffer, 0, 20);
                    VMDIKEnable vmdIKEnable = new VMDIKEnable
                    {
                        //Shift_JISでヌル文字除去
                        
                        IKName = ToEncoding.ToUnicode(buffer).TrimEnd('\0').TrimEnd('?').TrimEnd('\0'),
                        Enable = BitConverter.ToBoolean(new byte[] { binaryReader.ReadByte() }, 0)
                    };
                    IKEnable.Add(vmdIKEnable);
                }
            }
        };

        public string MotionName = "None";
        public float Version = -1;
        //最終フレーム
        public int FrameCount = -1;

        //人ボーンのキーフレームのリスト
        public List<BoneKeyFrame> BoneKeyFrames = new List<BoneKeyFrame>();
        //表情モーフのキーフレームのリスト
        public List<FaceKeyFrame> FaceKeyFrames = new List<FaceKeyFrame>();
        //カメラのキーフレームのリスト
        public List<CameraKeyFrame> CameraFrames = new List<CameraKeyFrame>();
        //照明のキーフレームのリスト
        public List<LightKeyFrame> LightFrames = new List<LightKeyFrame>();
        //セルフ影のキーフレームのリスト
        public List<SelfShadowKeyFrame> SelfShadowKeyFrames = new List<SelfShadowKeyFrame>();
        //IKのキーフレームのリスト
        public List<IKKeyFrame> IKFrames = new List<IKKeyFrame>();

        public VMD() { }
        public VMD(string filePath) { LoadFromFile(filePath); }

        public void LoadFromStream(BinaryReader binaryReader)
        {
            try
            {
                char[] buffer = new char[30];

                // 读取文件类型
                string RightFileType = "Vocaloid Motion Data";
                byte[] fileTypeBytes = binaryReader.ReadBytes(30);
                string fileType = ToEncoding.ToUnicode(fileTypeBytes).Substring(0, RightFileType.Length);
                if (!fileType.Equals("Vocaloid Motion Data"))
                {
                    Console.WriteLine("要读取的文件不是VMD文件");
                }

                //バージョンの読み込み、バージョンは後で使用していない
                Version = BitConverter.ToSingle((from c in buffer select Convert.ToByte(c)).ToArray(), 0);

                //モーション名の読み込み、Shift_JISで保存されている
                byte[] nameBytes = binaryReader.ReadBytes(20);
                MotionName = ToEncoding.ToUnicode(nameBytes);
                //ヌル文字除去
                MotionName = MotionName.TrimEnd('\0').TrimEnd('?').TrimEnd('\0');

                //人ボーンのキーフレームの読み込み
                int boneFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < boneFrameCount; i++)
                {
                    BoneKeyFrames.Add(new BoneKeyFrame(binaryReader));
                }

                //表情モーフのキーフレームの読み込み
                int faceFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < faceFrameCount; i++)
                {
                    FaceKeyFrames.Add(new FaceKeyFrame(binaryReader));
                }
                FaceKeyFrames = FaceKeyFrames.OrderBy(x => x.FrameNumber).ToList();

                //カメラのキーフレームの読み込み
                int cameraFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < cameraFrameCount; i++)
                {
                    CameraFrames.Add(new CameraKeyFrame(binaryReader));
                }
                CameraFrames = CameraFrames.OrderBy(x => x.Frame).ToList();

                //照明のキーフレームの読み込み
                int lightFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < lightFrameCount; i++)
                {
                    LightFrames.Add(new LightKeyFrame(binaryReader));
                }
                LightFrames = LightFrames.OrderBy(x => x.Frame).ToList();

                //vmdのバージョンによってはここで終わる
                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length) { return; }

                //セルフシャドウの読み込み
                int selfShadowFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < selfShadowFrameCount; i++)
                {
                    SelfShadowKeyFrames.Add(new SelfShadowKeyFrame(binaryReader));
                }
                SelfShadowKeyFrames = SelfShadowKeyFrames.OrderBy(x => x.Frame).ToList();

                //vmdのバージョンによってはここで終わる
                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length) { return; }

                //IKのキーフレームの読み込み
                int ikFrameCount = binaryReader.ReadInt32();
                for (int i = 0; i < ikFrameCount; i++)
                {
                    IKFrames.Add(new IKKeyFrame(binaryReader));
                }

                //ここで終わってないとおかしい
                if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                {
                    Console.WriteLine("数据末尾有未知部分");
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VMD读取错误:\n{ex}");
                return;
            }
        }
        public void LoadFromFile(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                LoadFromStream(binaryReader);
            }
        }
    };

    class Util
    {
        public static Vector3 ReadVector3(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();
            return new Vector3(x, y, z);
        }

        public static Quaternion ReadQuaternion(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();
            float w = binaryReader.ReadSingle();

            return new Quaternion(x, y, z, w);
        }

        public static Quaternion ReadEulerQuaternion(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();

            return Quaternion.Euler(x, y, z);
        }
    }
}