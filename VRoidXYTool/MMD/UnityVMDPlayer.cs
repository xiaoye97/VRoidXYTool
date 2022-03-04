using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static VRoidXYTool.MMD.VMDReader.BoneKeyFrameGroup;
using System.Collections;
using System.Threading.Tasks;

namespace VRoidXYTool.MMD
{
    public class UnityVMDPlayer : MonoBehaviour
    {
        public VMDReader VMDReader { get; private set; }
        public bool IsPlaying { get; private set; } = false;
        //IsEndは再生が終了したことを示すフラグで、何の処理にも使用されていない
        public bool IsEnd { get; private set; } = false;
        public int FrameNumber { get; private set; } = 0;
        //Start終了時に実行させる
        Action startAction = () => { };
        //モーション終了時に実行させる
        Action endAction = () => { };
        const float DefaultFPS = 30f;
        public static float FPS { get; private set; } = DefaultFPS;
        //ボーン移動量の補正係数
        //この値は大体の値、改良の余地あり
        const float DefaultBoneAmplifier = 0.08f;
        //エラー値の再現に用いる
        readonly Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

        //以下はStart時に初期化
        //animatorはPlay時にも一応初期化
        public Animator Animator { get; private set; }

        //全てのボーンを名前で引く辞書
        Dictionary<string, Transform> transformDictionary = new Dictionary<string, Transform>();
        //人ボーンを人ボーン名で引く辞書,Startで代入
        Dictionary<BoneNames, Transform> humanBoneTransformDictionary;
        //FPSのずれを補間するための辞書
        Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)> nowPose
            = new Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)>();
        Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)> nextPose
            = new Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)>();

        int lastFrameNumber = -1;
        //FPSのずれによる非整数のフレームも計算する
        float internalFrameNumber = 0;
        //以下はPlay時に初期化
        UpperBodyAnimation upperBodyAnimation;
        LowerBodyAnimation lowerBodyAnimation;
        CenterAnimation centerAnimation;
        FootIK leftFootIK;
        FootIK rightFootIK;
        ToeIK leftToeIK;
        ToeIK rightToeIK;
        BoneGhost boneGhost;
        MorphPlayer morphPlayer;

        //以下はインスペクタにて設定
        public bool IsLoop = false;
        //全ての親はデフォルトでオン
        public bool UseParentOfAll = true;
        public Transform LeftUpperArmTwist;
        public Transform RightUpperArmTwist;
        //VMDファイルのパスを与えて再生するまでオフセットは更新されない
        public Vector3 LeftFootOffset = new Vector3(0, 0, 0);
        public Vector3 RightFootOffset = new Vector3(0, 0, 0);

        // Start is called before the first frame update
        void Start()
        {
            Animator = GetComponent<Animator>();

            //子孫のボーンを記録
            makeTransformDictionary(Animator.transform, transformDictionary);
            //対応するボーンを記録
            humanBoneTransformDictionary = new Dictionary<BoneNames, Transform>()
            {
                //下半身などというものはUnityにはない
                { BoneNames.センター, (Animator.GetBoneTransform(HumanBodyBones.Hips))},
                { BoneNames.全ての親, (Animator.GetBoneTransform(HumanBodyBones.Hips).parent)},
                { BoneNames.上半身,   (Animator.GetBoneTransform(HumanBodyBones.Spine))},
                { BoneNames.上半身2,  (Animator.GetBoneTransform(HumanBodyBones.Chest))},
                { BoneNames.頭,       (Animator.GetBoneTransform(HumanBodyBones.Head))},
                { BoneNames.首,       (Animator.GetBoneTransform(HumanBodyBones.Neck))},
                { BoneNames.左肩,     (Animator.GetBoneTransform(HumanBodyBones.LeftShoulder))},
                { BoneNames.右肩,     (Animator.GetBoneTransform(HumanBodyBones.RightShoulder))},
                { BoneNames.左腕,     (Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm))},
                { BoneNames.右腕,     (Animator.GetBoneTransform(HumanBodyBones.RightUpperArm))},
                { BoneNames.左ひじ,   (Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm))},
                { BoneNames.右ひじ,   (Animator.GetBoneTransform(HumanBodyBones.RightLowerArm))},
                { BoneNames.左手首,   (Animator.GetBoneTransform(HumanBodyBones.LeftHand))},
                { BoneNames.右手首,   (Animator.GetBoneTransform(HumanBodyBones.RightHand))},
                { BoneNames.左親指１, (Animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal))},
                { BoneNames.右親指１, (Animator.GetBoneTransform(HumanBodyBones.RightThumbProximal))},
                { BoneNames.左親指２, (Animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate))},
                { BoneNames.右親指２, (Animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate))},
                { BoneNames.左人指１, (Animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal))},
                { BoneNames.右人指１, (Animator.GetBoneTransform(HumanBodyBones.RightIndexProximal))},
                { BoneNames.左人指２, (Animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate))},
                { BoneNames.右人指２, (Animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate))},
                { BoneNames.左人指３, (Animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal))},
                { BoneNames.右人指３, (Animator.GetBoneTransform(HumanBodyBones.RightIndexDistal))},
                { BoneNames.左中指１, (Animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal))},
                { BoneNames.右中指１, (Animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal))},
                { BoneNames.左中指２, (Animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate))},
                { BoneNames.右中指２, (Animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate))},
                { BoneNames.左中指３, (Animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal))},
                { BoneNames.右中指３, (Animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal))},
                { BoneNames.左薬指１, (Animator.GetBoneTransform(HumanBodyBones.LeftRingProximal))},
                { BoneNames.右薬指１, (Animator.GetBoneTransform(HumanBodyBones.RightRingProximal))},
                { BoneNames.左薬指２, (Animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate))},
                { BoneNames.右薬指２, (Animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate))},
                { BoneNames.左薬指３, (Animator.GetBoneTransform(HumanBodyBones.LeftRingDistal))},
                { BoneNames.右薬指３, (Animator.GetBoneTransform(HumanBodyBones.RightRingDistal))},
                { BoneNames.左小指１, (Animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal))},
                { BoneNames.右小指１, (Animator.GetBoneTransform(HumanBodyBones.RightLittleProximal))},
                { BoneNames.左小指２, (Animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate))},
                { BoneNames.右小指２, (Animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate))},
                { BoneNames.左小指３, (Animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal))},
                { BoneNames.右小指３, (Animator.GetBoneTransform(HumanBodyBones.RightLittleDistal))},
                { BoneNames.左足ＩＫ, (Animator.GetBoneTransform(HumanBodyBones.LeftFoot))},
                { BoneNames.右足ＩＫ, (Animator.GetBoneTransform(HumanBodyBones.RightFoot))},
                { BoneNames.左足,     (Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg))},
                { BoneNames.右足,     (Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg))},
                { BoneNames.左ひざ,   (Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))},
                { BoneNames.右ひざ,   (Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg))},
                { BoneNames.左足首,   (Animator.GetBoneTransform(HumanBodyBones.LeftFoot))},
                { BoneNames.右足首,   (Animator.GetBoneTransform(HumanBodyBones.RightFoot))},
                //左つま先, 右つま先は情報付けると足首の回転、位置との矛盾が生じかねない
                //{ BoneNames.左つま先,   (Animator.GetBoneTransform(HumanBodyBones.LeftToes))},
                //{ BoneNames.右つま先,   (Animator.GetBoneTransform(HumanBodyBones.RightToes))}
        };
            //Console.WriteLine($"开始输出骨骼信息");
            //foreach (var kv in humanBoneTransformDictionary)
            //{
            //    Console.WriteLine($"{kv.Key}\t{kv.Value.name}");
            //}

            startAction();

            void makeTransformDictionary(Transform rootBone, Dictionary<string, Transform> dictionary)
            {
                if (dictionary.ContainsKey(rootBone.name)) { return; }
                dictionary.Add(rootBone.name, rootBone);
                foreach (Transform childT in rootBone)
                {
                    makeTransformDictionary(childT, dictionary);
                }
            }
        }

        void Update()
        {
            if (!IsPlaying) { return; }
            if (VMDReader == null) { return; }

            //最終フレームを超えれば終了
            if (VMDReader.FrameCount <= FrameNumber)
            {
                if (IsLoop)
                {
                    JumpToFrame(0);
                    return;
                }

                IsPlaying = false;
                IsEnd = true;
                //最後にすることがあれば
                endAction();
                return;
            }

            if (FrameNumber != lastFrameNumber)
            {
                lastFrameNumber = FrameNumber;
                AnimateBody(FrameNumber);
                nowPose = SavePose();
                AnimateBody(FrameNumber + 1);
                nextPose = SavePose();

                if (morphPlayer != null) { morphPlayer.Morph(FrameNumber); }
            }
            float rate = internalFrameNumber - FrameNumber;

            foreach (BoneNames boneName in nowPose.Keys)
            {
                if (humanBoneTransformDictionary[boneName] == null) { continue; }
                if (!nextPose.ContainsKey(boneName)) { continue; }

                humanBoneTransformDictionary[boneName].localPosition = Vector3.Lerp(nowPose[boneName].localPosition, nextPose[boneName].localPosition, rate);
                humanBoneTransformDictionary[boneName].localRotation = Quaternion.Lerp(nowPose[boneName].localRotation, nextPose[boneName].localRotation, rate);
            }

            internalFrameNumber += FPS * Time.deltaTime;
            FrameNumber = (int)internalFrameNumber;
        }

        public static void SetFPS(int fps)
        {
            FPS = fps;
        }

        void OnDrawGizmosSelected()
        {
            Animator = GetComponent<Animator>();
            Transform leftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Gizmos.DrawWireSphere(leftFoot.position + leftFoot.rotation * new Vector3(LeftFootOffset.x, -LeftFootOffset.y, LeftFootOffset.z), 0.1f);
            Gizmos.DrawWireSphere(rightFoot.position + rightFoot.rotation * new Vector3(RightFootOffset.x, -RightFootOffset.y, RightFootOffset.z), 0.1f);
        }

        public void SetEndAction(Action endAction)
        {
            this.endAction = endAction;
        }
        public void SetStartAction(Action startAction)
        {
            this.startAction = startAction;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsEnd = true;
            Animator = GetComponent<Animator>();
            Animator.enabled = true;
            FrameNumber = 0;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Play()
        {
            Console.WriteLine($"开始正式播放");
            IsEnd = false;
            IsPlaying = true;
        }

        public void Play(Action endAction)
        {
            this.endAction = endAction;
            Play();
        }

        //こいつがPlayの本体みたいなもの
        public void Play(VMDReader vmdReader)
        {
            Animator = GetComponent<Animator>();
            Animator.enabled = false;

            //モデルに初期ポーズを取らせる
            EnforceInitialPose(Animator, true);

            this.VMDReader = vmdReader;
            if (boneGhost != null) { boneGhost.Destroy(); }
            boneGhost = new BoneGhost(Animator, humanBoneTransformDictionary);
            morphPlayer = new MorphPlayer(transform, vmdReader);
            upperBodyAnimation = new UpperBodyAnimation(Animator, vmdReader, boneGhost, LeftUpperArmTwist, RightUpperArmTwist);
            lowerBodyAnimation = new LowerBodyAnimation(Animator, vmdReader, boneGhost);
            centerAnimation = new CenterAnimation(vmdReader, Animator, boneGhost);
            if (leftFootIK != null) { leftFootIK.Destroy(); }
            leftFootIK = new FootIK(vmdReader, Animator, FootIK.Feet.LeftFoot, LeftFootOffset);
            if (rightFootIK != null) { rightFootIK.Destroy(); }
            rightFootIK = new FootIK(vmdReader, Animator, FootIK.Feet.RightFoot, RightFootOffset);
            if (leftToeIK != null) { leftToeIK.Destroy(); }
            leftToeIK = new ToeIK(vmdReader, Animator, boneGhost, ToeIK.Toes.LeftToe);
            if (rightToeIK != null) { rightToeIK.Destroy(); }
            rightToeIK = new ToeIK(vmdReader, Animator, boneGhost, ToeIK.Toes.RightToe);

            FrameNumber = 0;
            internalFrameNumber = 0;
            lastFrameNumber = -1;

            Play();
        }

        public void Play(VMDReader vmdReader, int frameNumber)
        {
            Console.WriteLine($"从第{frameNumber}帧开始播放");
            if (frameNumber < 0) { frameNumber = 0; }
            this.FrameNumber = frameNumber;
            Play(vmdReader);
        }

        public void Play(string filePath)
        {
            VMDReader = new VMDReader(filePath);

            Play(VMDReader, 0);
        }
        public async Task PlayAsync(string filePath)
        {
            VMDReader = await VMDReader.ReadVMDAsync(filePath);

            Play(VMDReader, 0);
        }

        /// <summary>
        /// 加载VMD但是不播放
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task LoadForPlay(string filePath)
        {
            VMDReader = await VMDReader.ReadVMDAsync(filePath);
        }

        public void Play(string filePath, Action endAction)
        {
            SetEndAction(endAction);
            Play(filePath);
        }
        public async Task PlayAsync(string filePath, Action endAction)
        {
            SetEndAction(endAction);
            await PlayAsync(filePath);
        }

        public void Play(string filePath, bool useParentOfAll)
        {
            UseParentOfAll = useParentOfAll;
            Play(filePath);
        }
        public async Task PlayAsync(string filePath, bool useParentOfAll)
        {
            UseParentOfAll = useParentOfAll;
            await PlayAsync(filePath);
        }

        public void Play(string filePath, bool useParentOfAll, Action endAction)
        {
            SetEndAction(endAction);
            Play(filePath, useParentOfAll);
        }
        public async Task PlayAsync(string filePath, bool useParentOfAll, Action endAction)
        {
            SetEndAction(endAction);
            await PlayAsync(filePath, useParentOfAll);
        }

        public void JumpToFrame(int frameNumber)
        {
            if (frameNumber < 0) { frameNumber = 0; }
            this.lastFrameNumber = -1;
            this.FrameNumber = frameNumber;
            this.internalFrameNumber = frameNumber;
            AnimateBody(frameNumber);
            if (morphPlayer != null) { morphPlayer.Morph(frameNumber); }
        }

        void AnimateBody(int frameNumber)
        {
            if (UseParentOfAll) { AnimateParentOfAll(); }
            if (UseParentOfAll) { InterpolateParentOfAll(); }
            if (upperBodyAnimation != null) { upperBodyAnimation.AnimateUpperBody(frameNumber); }
            if (upperBodyAnimation != null) { upperBodyAnimation.InterpolateUpperBody(frameNumber); }
            if (lowerBodyAnimation != null) { lowerBodyAnimation.AnimateLowerBody(frameNumber); }
            if (lowerBodyAnimation != null) { lowerBodyAnimation.InterpolateLowerBody(frameNumber); }
            if (centerAnimation != null) { centerAnimation.AnimateAndInterpolate(frameNumber); }
            if (centerAnimation != null) { centerAnimation.Complement(frameNumber); }
            if (boneGhost != null) { boneGhost.GhostParentOfAll(); }
            if (boneGhost != null) { boneGhost.GhostAllChildren(); }
            if (leftFootIK != null) { leftFootIK.IK(frameNumber); }
            if (rightFootIK != null) { rightFootIK.IK(frameNumber); }
            if (leftFootIK != null) { leftFootIK.InterpolateIK(frameNumber); }
            if (rightFootIK != null) { rightFootIK.InterpolateIK(frameNumber); }
            if (leftToeIK != null) { leftToeIK.IK(frameNumber); }
            if (rightToeIK != null) { rightToeIK.IK(frameNumber); }
            if (leftToeIK != null) { leftToeIK.InterpolateIK(frameNumber); }
            if (rightToeIK != null) { rightToeIK.InterpolateIK(frameNumber); }
        }

        Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)> SavePose()
        {
            Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)> pose
                = new Dictionary<BoneNames, (Vector3 localPosition, Quaternion localRotation)>();

            foreach (BoneNames boneName in humanBoneTransformDictionary.Keys)
            {
                if (humanBoneTransformDictionary[boneName] == null) { continue; }
                pose.Add(boneName, (humanBoneTransformDictionary[boneName].localPosition, humanBoneTransformDictionary[boneName].localRotation));
            }

            return pose;
        }

        void AnimateParentOfAll()
        {
            if (boneGhost == null) { return; }
            VMD.BoneKeyFrame parentBoneFrame = VMDReader.GetBoneKeyFrame(BoneNames.全ての親, FrameNumber);
            if (parentBoneFrame == null) { parentBoneFrame = new VMD.BoneKeyFrame(); }
            if (parentBoneFrame.Position != Vector3.zero)
            {
                boneGhost.ParentOfAllGhost.localPosition = parentBoneFrame.Position * DefaultBoneAmplifier;
            }
            if (parentBoneFrame.Rotation != ZeroQuaternion)
            {
                boneGhost.ParentOfAllGhost.localRotation = Quaternion.identity.PlusRotation(parentBoneFrame.Rotation);
            }
        }

        void InterpolateParentOfAll()
        {
            VMDReader.BoneKeyFrameGroup vmdBoneFrameGroup = VMDReader.GetBoneKeyFrameGroup(BoneNames.全ての親);
            VMD.BoneKeyFrame lastPositionVMDBoneFrame = vmdBoneFrameGroup.LastPositionKeyFrame;
            VMD.BoneKeyFrame lastRotationVMDBoneFrame = vmdBoneFrameGroup.LastRotationKeyFrame;
            VMD.BoneKeyFrame nextPositionVMDBoneFrame = vmdBoneFrameGroup.NextPositionKeyFrame;
            VMD.BoneKeyFrame nextRotationVMDBoneFrame = vmdBoneFrameGroup.NextRotationKeyFrame;

            boneGhost.ParentOfAllGhost.localPosition = VMDReader.InterporatePosition(VMDReader, BoneNames.全ての親, FrameNumber) * DefaultBoneAmplifier;

            boneGhost.ParentOfAllGhost.localRotation = VMDReader.InterporateRotation(VMDReader, BoneNames.全ての親, FrameNumber);
        }

        void EnforceInitialPose(Animator animator, bool aPose = false)
        {
            if (animator == null)
            {
                Console.WriteLine("EnforceInitialPose");
                Console.WriteLine("Animatorがnullです");
                return;
            }

            const int APoseDegree = 30;

            Vector3 position = animator.transform.position;
            Quaternion rotation = animator.transform.rotation;
            animator.transform.position = Vector3.zero;
            animator.transform.rotation = Quaternion.identity;

            int count = animator.avatar.humanDescription.skeleton.Length;
            for (int i = 0; i < count; i++)
            {
                if (!transformDictionary.ContainsKey(animator.avatar.humanDescription.skeleton[i].name))
                {
                    continue;
                }

                transformDictionary[animator.avatar.humanDescription.skeleton[i].name].localPosition
                    = animator.avatar.humanDescription.skeleton[i].position;
                transformDictionary[animator.avatar.humanDescription.skeleton[i].name].localRotation
                    = animator.avatar.humanDescription.skeleton[i].rotation;
            }

            animator.transform.position = position;
            animator.transform.rotation = rotation;

            if (aPose && animator.isHuman)
            {
                Transform leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                if (leftUpperArm == null || rightUpperArm == null) { return; }
                leftUpperArm.Rotate(animator.transform.forward, APoseDegree, Space.World);
                rightUpperArm.Rotate(animator.transform.forward, -APoseDegree, Space.World);
            }
        }

        class UpperBodyAnimation
        {
            Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

            Dictionary<BoneNames, (Transform, float)> upperBoneTransformDictionary;
            Dictionary<BoneNames, Vector3> upperBoneOriginalLocalPositions;
            Dictionary<BoneNames, Quaternion> upperBoneOriginalLocalRotations;

            VMDReader vmdReader;
            BoneGhost boneGhost;
            Transform LeftUpperArmTwist;
            Transform RightUpperArmTwist;

            public UpperBodyAnimation(Animator animator, VMDReader vmdReader, BoneGhost boneGhost, Transform leftUpperArmTwist, Transform rightUpperArmTwist)
            {
                this.vmdReader = vmdReader;
                this.boneGhost = boneGhost;
                LeftUpperArmTwist = leftUpperArmTwist;
                RightUpperArmTwist = rightUpperArmTwist;

                upperBoneTransformDictionary = new Dictionary<BoneNames, (Transform, float)>()
        {
            //センターはHips
            //下半身などというものはUnityにはないので、センターとともに処理
            { BoneNames.上半身 ,   (animator.GetBoneTransform(HumanBodyBones.Spine), DefaultBoneAmplifier) },
            { BoneNames.上半身2 ,  (animator.GetBoneTransform(HumanBodyBones.Chest), DefaultBoneAmplifier) },
            { BoneNames.頭 ,       (animator.GetBoneTransform(HumanBodyBones.Head), DefaultBoneAmplifier) },
            { BoneNames.首 ,       (animator.GetBoneTransform(HumanBodyBones.Neck), DefaultBoneAmplifier) },
            { BoneNames.左肩 ,     (animator.GetBoneTransform(HumanBodyBones.LeftShoulder), DefaultBoneAmplifier) },
            { BoneNames.右肩 ,     (animator.GetBoneTransform(HumanBodyBones.RightShoulder), DefaultBoneAmplifier) },
            { BoneNames.左腕 ,     (animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), DefaultBoneAmplifier) },
            { BoneNames.右腕 ,     (animator.GetBoneTransform(HumanBodyBones.RightUpperArm), DefaultBoneAmplifier) },
            { BoneNames.左ひじ ,   (animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), DefaultBoneAmplifier) },
            { BoneNames.右ひじ ,   (animator.GetBoneTransform(HumanBodyBones.RightLowerArm), DefaultBoneAmplifier) },
            { BoneNames.左手首 ,   (animator.GetBoneTransform(HumanBodyBones.LeftHand), DefaultBoneAmplifier) },
            { BoneNames.右手首 ,   (animator.GetBoneTransform(HumanBodyBones.RightHand), DefaultBoneAmplifier) },
            { BoneNames.左つま先 , (animator.GetBoneTransform(HumanBodyBones.LeftToes), DefaultBoneAmplifier) },
            { BoneNames.右つま先 , (animator.GetBoneTransform(HumanBodyBones.RightToes), DefaultBoneAmplifier) },
            { BoneNames.左親指１ , (animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal), DefaultBoneAmplifier) },
            { BoneNames.右親指１ , (animator.GetBoneTransform(HumanBodyBones.RightThumbProximal), DefaultBoneAmplifier) },
            { BoneNames.左親指２ , (animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate), DefaultBoneAmplifier) },
            { BoneNames.右親指２ , (animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate), DefaultBoneAmplifier) },
            { BoneNames.左人指１ , (animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal), DefaultBoneAmplifier) },
            { BoneNames.右人指１ , (animator.GetBoneTransform(HumanBodyBones.RightIndexProximal), DefaultBoneAmplifier) },
            { BoneNames.左人指２ , (animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate), DefaultBoneAmplifier) },
            { BoneNames.右人指２ , (animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), DefaultBoneAmplifier) },
            { BoneNames.左人指３ , (animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal), DefaultBoneAmplifier) },
            { BoneNames.右人指３ , (animator.GetBoneTransform(HumanBodyBones.RightIndexDistal), DefaultBoneAmplifier) },
            { BoneNames.左中指１ , (animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), DefaultBoneAmplifier) },
            { BoneNames.右中指１ , (animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), DefaultBoneAmplifier) },
            { BoneNames.左中指２ , (animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), DefaultBoneAmplifier) },
            { BoneNames.右中指２ , (animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), DefaultBoneAmplifier) },
            { BoneNames.左中指３ , (animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), DefaultBoneAmplifier) },
            { BoneNames.右中指３ , (animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), DefaultBoneAmplifier) },
            { BoneNames.左薬指１ , (animator.GetBoneTransform(HumanBodyBones.LeftRingProximal), DefaultBoneAmplifier) },
            { BoneNames.右薬指１ , (animator.GetBoneTransform(HumanBodyBones.RightRingProximal), DefaultBoneAmplifier) },
            { BoneNames.左薬指２ , (animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate), DefaultBoneAmplifier) },
            { BoneNames.右薬指２ , (animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate), DefaultBoneAmplifier) },
            { BoneNames.左薬指３ , (animator.GetBoneTransform(HumanBodyBones.LeftRingDistal), DefaultBoneAmplifier) },
            { BoneNames.右薬指３ , (animator.GetBoneTransform(HumanBodyBones.RightRingDistal), DefaultBoneAmplifier) },
            { BoneNames.左小指１ , (animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), DefaultBoneAmplifier) },
            { BoneNames.右小指１ , (animator.GetBoneTransform(HumanBodyBones.RightLittleProximal), DefaultBoneAmplifier) },
            { BoneNames.左小指２ , (animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), DefaultBoneAmplifier) },
            { BoneNames.右小指２ , (animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), DefaultBoneAmplifier) },
            { BoneNames.左小指３ , (animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal), DefaultBoneAmplifier) },
            { BoneNames.右小指３ , (animator.GetBoneTransform(HumanBodyBones.RightLittleDistal), DefaultBoneAmplifier) },
        };
                if (LeftUpperArmTwist == null)
                {
                    upperBoneTransformDictionary.Add(BoneNames.左腕捩れ, (LeftUpperArmTwist, DefaultBoneAmplifier));
                }
                if (RightUpperArmTwist == null)
                {
                    upperBoneTransformDictionary.Add(BoneNames.右腕捩れ, (RightUpperArmTwist, DefaultBoneAmplifier));
                }

                //モデルの初期ポーズを保存
                upperBoneOriginalLocalPositions = new Dictionary<BoneNames, Vector3>();
                upperBoneOriginalLocalRotations = new Dictionary<BoneNames, Quaternion>();
                int count = VMDReader.BoneKeyFrameGroup.StringBoneNames.Count;
                for (int i = 0; i < count; i++)
                {
                    BoneNames boneName = (BoneNames)VMDReader.BoneKeyFrameGroup.StringBoneNames.IndexOf(VMDReader.BoneKeyFrameGroup.StringBoneNames[i]);
                    if (!upperBoneTransformDictionary.Keys.Contains(boneName) || upperBoneTransformDictionary[boneName].Item1 == null) { continue; }
                    upperBoneOriginalLocalPositions.Add(boneName, upperBoneTransformDictionary[boneName].Item1.localPosition);
                    upperBoneOriginalLocalRotations.Add(boneName, upperBoneTransformDictionary[boneName].Item1.localRotation);
                }
            }

            public void AnimateUpperBody(int frameNumber)
            {
                foreach (BoneNames boneName in upperBoneTransformDictionary.Keys)
                {
                    Transform boneTransform = upperBoneTransformDictionary[boneName].Item1;
                    if (boneTransform == null) { continue; }

                    VMD.BoneKeyFrame vmdBoneFrame = vmdReader.GetBoneKeyFrame(boneName, frameNumber);

                    if (vmdBoneFrame == null) { continue; }

                    if (boneGhost.GhostDictionary.Keys.Contains(boneName)
                        && boneGhost.GhostDictionary[boneName].enabled
                        && boneGhost.GhostDictionary[boneName].ghost != null)
                    {
                        if (vmdBoneFrame.Position != Vector3.zero)
                        {
                            boneGhost.GhostDictionary[boneName].ghost.localPosition = boneGhost.OriginalGhostLocalPositionDictionary[boneName] + vmdBoneFrame.Position * upperBoneTransformDictionary[boneName].Item2;
                        }
                        if (vmdBoneFrame.Rotation != ZeroQuaternion)
                        {
                            //Ghostは正規化されている
                            boneGhost.GhostDictionary[boneName].ghost.localRotation = Quaternion.identity.PlusRotation(vmdBoneFrame.Rotation);
                        }
                    }
                }
            }

            public void InterpolateUpperBody(int frameNumber)
            {
                foreach (BoneNames boneName in upperBoneTransformDictionary.Keys)
                {
                    Transform boneTransform = upperBoneTransformDictionary[boneName].Item1;
                    if (boneTransform == null) { continue; }

                    VMDReader.BoneKeyFrameGroup vmdBoneFrameGroup = vmdReader.GetBoneKeyFrameGroup(boneName);
                    VMD.BoneKeyFrame lastPositionVMDBoneFrame = vmdBoneFrameGroup.LastPositionKeyFrame;
                    VMD.BoneKeyFrame lastRotationVMDBoneFrame = vmdBoneFrameGroup.LastRotationKeyFrame;
                    VMD.BoneKeyFrame nextPositionVMDBoneFrame = vmdBoneFrameGroup.NextPositionKeyFrame;
                    VMD.BoneKeyFrame nextRotationVMDBoneFrame = vmdBoneFrameGroup.NextRotationKeyFrame;

                    if (boneGhost.GhostDictionary.Keys.Contains(boneName)
                        && boneGhost.GhostDictionary[boneName].enabled
                        && boneGhost.GhostDictionary[boneName].ghost != null)
                    {
                        boneGhost.GhostDictionary[boneName].ghost.localPosition
                            = boneGhost.OriginalGhostLocalPositionDictionary[boneName]
                            + VMDReader.InterporatePosition(vmdReader, boneName, frameNumber) * DefaultBoneAmplifier;

                        boneGhost.GhostDictionary[boneName].ghost.localRotation = VMDReader.InterporateRotation(vmdReader, boneName, frameNumber);
                    }
                }
            }
        }

        class LowerBodyAnimation
        {
            Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

            Dictionary<BoneNames, (Transform, float)> lowerBoneTransformDictionary;
            Dictionary<BoneNames, Vector3> lowerBoneOriginalLocalPositions;
            Dictionary<BoneNames, Quaternion> lowerBoneOriginalLocalRotations;

            VMDReader vmdReader;
            BoneGhost boneGhost;

            public LowerBodyAnimation(Animator animator, VMDReader vmdReader, BoneGhost boneGhost)
            {
                this.vmdReader = vmdReader;
                this.boneGhost = boneGhost;

                lowerBoneTransformDictionary = new Dictionary<BoneNames, (Transform, float)>()
        {
            //センターはHips
            //下半身などというものはUnityにはないので、センターとともに処理
            { BoneNames.右足,   (animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), DefaultBoneAmplifier) },
            { BoneNames.右ひざ ,  (animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), DefaultBoneAmplifier) },
            { BoneNames.右足首 ,       (animator.GetBoneTransform(HumanBodyBones.RightFoot), DefaultBoneAmplifier) },
            { BoneNames.左足 ,       (animator.GetBoneTransform(HumanBodyBones.Neck), DefaultBoneAmplifier) },
            { BoneNames.左ひざ ,     (animator.GetBoneTransform(HumanBodyBones.LeftShoulder), DefaultBoneAmplifier) },
            { BoneNames.左足首 ,     (animator.GetBoneTransform(HumanBodyBones.RightShoulder), DefaultBoneAmplifier) }
        };

                //モデルの初期ポーズを保存
                lowerBoneOriginalLocalPositions = new Dictionary<BoneNames, Vector3>();
                lowerBoneOriginalLocalRotations = new Dictionary<BoneNames, Quaternion>();
                int count = VMDReader.BoneKeyFrameGroup.StringBoneNames.Count;
                for (int i = 0; i < count; i++)
                {
                    BoneNames boneName = (BoneNames)VMDReader.BoneKeyFrameGroup.StringBoneNames.IndexOf(VMDReader.BoneKeyFrameGroup.StringBoneNames[i]);
                    if (!lowerBoneTransformDictionary.Keys.Contains(boneName) || lowerBoneTransformDictionary[boneName].Item1 == null) { continue; }
                    lowerBoneOriginalLocalPositions.Add(boneName, lowerBoneTransformDictionary[boneName].Item1.localPosition);
                    lowerBoneOriginalLocalRotations.Add(boneName, lowerBoneTransformDictionary[boneName].Item1.localRotation);
                }
            }

            public void AnimateLowerBody(int frameNumber)
            {
                foreach (BoneNames boneName in lowerBoneTransformDictionary.Keys)
                {
                    Transform boneTransform = lowerBoneTransformDictionary[boneName].Item1;
                    if (boneTransform == null) { continue; }

                    VMD.BoneKeyFrame vmdBoneFrame = vmdReader.GetBoneKeyFrame(boneName, frameNumber);

                    if (vmdBoneFrame == null) { continue; }

                    if (boneGhost.GhostDictionary.Keys.Contains(boneName)
                        && boneGhost.GhostDictionary[boneName].ghost != null)
                    {
                        if (vmdBoneFrame.Position != Vector3.zero)
                        {
                            boneGhost.GhostDictionary[boneName].ghost.localPosition = boneGhost.OriginalGhostLocalPositionDictionary[boneName] + vmdBoneFrame.Position * lowerBoneTransformDictionary[boneName].Item2;
                        }
                        if (vmdBoneFrame.Rotation != ZeroQuaternion)
                        {
                            //Ghostは正規化されている
                            boneGhost.GhostDictionary[boneName].ghost.localRotation = Quaternion.identity.PlusRotation(vmdBoneFrame.Rotation);
                        }
                    }
                }
            }

            public void InterpolateLowerBody(int frameNumber)
            {
                foreach (BoneNames boneName in lowerBoneTransformDictionary.Keys)
                {
                    Transform boneTransform = lowerBoneTransformDictionary[boneName].Item1;
                    if (boneTransform == null) { continue; }

                    VMDReader.BoneKeyFrameGroup vmdBoneFrameGroup = vmdReader.GetBoneKeyFrameGroup(boneName);
                    VMD.BoneKeyFrame lastPositionVMDBoneFrame = vmdBoneFrameGroup.LastPositionKeyFrame;
                    VMD.BoneKeyFrame lastRotationVMDBoneFrame = vmdBoneFrameGroup.LastRotationKeyFrame;
                    VMD.BoneKeyFrame nextPositionVMDBoneFrame = vmdBoneFrameGroup.NextPositionKeyFrame;
                    VMD.BoneKeyFrame nextRotationVMDBoneFrame = vmdBoneFrameGroup.NextRotationKeyFrame;

                    if (boneGhost.GhostDictionary.Keys.Contains(boneName)
                        && boneGhost.GhostDictionary[boneName].ghost != null)
                    {
                        boneGhost.GhostDictionary[boneName].ghost.localPosition
                            = boneGhost.OriginalGhostLocalPositionDictionary[boneName]
                            + VMDReader.InterporatePosition(vmdReader, boneName, frameNumber) * DefaultBoneAmplifier;

                        boneGhost.GhostDictionary[boneName].ghost.localRotation = VMDReader.InterporateRotation(vmdReader, boneName, frameNumber);
                    }
                }
            }
        }

        //VMDではセンターはHipの差分のみの位置、回転情報を持つ
        //Unityにない下半身ボーンの処理もここで行う
        class CenterAnimation
        {
            readonly Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

            BoneNames centerBoneName = BoneNames.センター;
            BoneNames grooveBoneName = BoneNames.グルーブ;

            public VMDReader VMDReader { get; private set; }

            public Animator Animator { get; private set; }
            Transform hips;
            BoneGhost boneGhost;

            public CenterAnimation(VMDReader vmdReader, Animator animator, BoneGhost boneGhost)
            {
                VMDReader = vmdReader;
                Animator = animator;
                hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
                this.boneGhost = boneGhost;
            }

            public void AnimateAndInterpolate(int frameNumber)
            {
                if (!boneGhost.GhostDictionary.Keys.Contains(centerBoneName)
                    || !boneGhost.GhostDictionary[centerBoneName].enabled
                    || boneGhost.GhostDictionary[centerBoneName].ghost == null)
                { return; }

                //センター、グルーブの処理を行う
                VMD.BoneKeyFrame centerVMDBoneFrame = VMDReader.GetBoneKeyFrame(centerBoneName, frameNumber);
                VMD.BoneKeyFrame grooveVMDBoneFrame = VMDReader.GetBoneKeyFrame(grooveBoneName, frameNumber);

                //初期化
                boneGhost.GhostDictionary[centerBoneName].ghost.localPosition
                    = boneGhost.OriginalGhostLocalPositionDictionary[centerBoneName];
                boneGhost.GhostDictionary[centerBoneName].ghost.localRotation
                    = Quaternion.identity;

                if (centerVMDBoneFrame != null && centerVMDBoneFrame.Position != Vector3.zero)
                {
                    boneGhost.GhostDictionary[BoneNames.センター].ghost.localPosition
                        += centerVMDBoneFrame.Position * DefaultBoneAmplifier;
                }
                else
                {
                    boneGhost.GhostDictionary[centerBoneName].ghost.localPosition
                        += VMDReader.InterporatePosition(VMDReader, centerBoneName, frameNumber) * DefaultBoneAmplifier;
                }


                if (centerVMDBoneFrame != null && centerVMDBoneFrame.Rotation != ZeroQuaternion)
                {
                    //Ghostは正規化されている
                    boneGhost.GhostDictionary[centerBoneName].ghost.localRotation =
                        boneGhost.GhostDictionary[centerBoneName].ghost.localRotation
                        .PlusRotation(centerVMDBoneFrame.Rotation);
                }
                else
                {
                    boneGhost.GhostDictionary[centerBoneName].ghost.localRotation
                        = VMDReader.InterporateRotation(VMDReader, centerBoneName, frameNumber);
                }

                if (grooveVMDBoneFrame != null && grooveVMDBoneFrame.Position != Vector3.zero)
                {
                    boneGhost.GhostDictionary[centerBoneName].ghost.localPosition
                        += grooveVMDBoneFrame.Position * DefaultBoneAmplifier;
                }
                else
                {
                    boneGhost.GhostDictionary[centerBoneName].ghost.localPosition
                        += VMDReader.InterporatePosition(VMDReader, grooveBoneName, frameNumber) * DefaultBoneAmplifier;
                }

                if (grooveVMDBoneFrame != null && grooveVMDBoneFrame.Rotation != ZeroQuaternion)
                {
                    //Ghostは正規化されている
                    boneGhost.GhostDictionary[centerBoneName].ghost.localRotation =
                        boneGhost.GhostDictionary[centerBoneName].ghost.localRotation
                        .PlusRotation(grooveVMDBoneFrame.Rotation);
                }
                else
                {
                    boneGhost.GhostDictionary[centerBoneName].ghost.localRotation =
                        boneGhost.GhostDictionary[centerBoneName].ghost.localRotation
                        .PlusRotation(VMDReader.InterporateRotation(VMDReader, grooveBoneName, frameNumber));
                }
            }

            //下半身の処理を行う
            public void Complement(int frameNumber)
            {
                //次に下半身の処理を行う、おそらく下半身に位置情報はない
                BoneNames lowerBodyBoneName = BoneNames.下半身;
                if (hips == null) { return; }
                VMD.BoneKeyFrame lowerBodyVMDBoneFrame = VMDReader.GetBoneKeyFrame(lowerBodyBoneName, frameNumber);
                VMDReader.BoneKeyFrameGroup lowerBodyVMDBoneGroup = VMDReader.GetBoneKeyFrameGroup(lowerBodyBoneName);

                if (boneGhost.GhostDictionary.Keys.Contains(BoneNames.上半身)
                && boneGhost.GhostDictionary[BoneNames.上半身].enabled
                && boneGhost.GhostDictionary[BoneNames.上半身].ghost != null
                && boneGhost.GhostDictionary.Keys.Contains(BoneNames.センター)
                && boneGhost.GhostDictionary[BoneNames.センター].enabled
                && boneGhost.GhostDictionary[BoneNames.センター].ghost != null)
                {
                    if (lowerBodyVMDBoneFrame != null && lowerBodyVMDBoneFrame.Position != Vector3.zero)
                    {
                        boneGhost.GhostDictionary[BoneNames.センター].ghost.localPosition += lowerBodyVMDBoneFrame.Position * DefaultBoneAmplifier;
                        boneGhost.GhostDictionary[BoneNames.上半身].ghost.position -= boneGhost.GhostDictionary[BoneNames.センター].ghost.rotation * lowerBodyVMDBoneFrame.Position * DefaultBoneAmplifier;
                    }
                    else
                    {
                        if (lowerBodyVMDBoneGroup == null) { return; }
                        VMD.BoneKeyFrame lastPositionVMDBoneFrame = lowerBodyVMDBoneGroup.LastPositionKeyFrame;
                        VMD.BoneKeyFrame nextPositionVMDBoneFrame = lowerBodyVMDBoneGroup.NextPositionKeyFrame;

                        if (nextPositionVMDBoneFrame != null && lastPositionVMDBoneFrame != null)
                        {
                            float xInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, lastPositionVMDBoneFrame.FrameNumber, nextPositionVMDBoneFrame.FrameNumber);
                            float yInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, lastPositionVMDBoneFrame.FrameNumber, nextPositionVMDBoneFrame.FrameNumber);
                            float zInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, lastPositionVMDBoneFrame.FrameNumber, nextPositionVMDBoneFrame.FrameNumber);

                            float xInterpolation = Mathf.Lerp(lastPositionVMDBoneFrame.Position.x, nextPositionVMDBoneFrame.Position.x, xInterpolationRate);
                            float yInterpolation = Mathf.Lerp(lastPositionVMDBoneFrame.Position.y, nextPositionVMDBoneFrame.Position.y, yInterpolationRate);
                            float zInterpolation = Mathf.Lerp(lastPositionVMDBoneFrame.Position.z, nextPositionVMDBoneFrame.Position.z, zInterpolationRate);

                            Vector3 deltaVector = new Vector3(xInterpolation, yInterpolation, zInterpolation) * DefaultBoneAmplifier;
                            boneGhost.GhostDictionary[BoneNames.センター].ghost.localPosition += deltaVector;
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.position -= boneGhost.GhostDictionary[BoneNames.センター].ghost.rotation * deltaVector;
                        }
                        else if (lastPositionVMDBoneFrame == null && nextPositionVMDBoneFrame != null)
                        {
                            float xInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, 0, nextPositionVMDBoneFrame.FrameNumber);
                            float yInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, 0, nextPositionVMDBoneFrame.FrameNumber);
                            float zInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, 0, nextPositionVMDBoneFrame.FrameNumber);

                            float xInterpolation = Mathf.Lerp(0, nextPositionVMDBoneFrame.Position.x, xInterpolationRate);
                            float yInterpolation = Mathf.Lerp(0, nextPositionVMDBoneFrame.Position.y, yInterpolationRate);
                            float zInterpolation = Mathf.Lerp(0, nextPositionVMDBoneFrame.Position.z, zInterpolationRate);
                            Vector3 deltaVector = new Vector3(xInterpolation, yInterpolation, zInterpolation) * DefaultBoneAmplifier;
                            boneGhost.GhostDictionary[BoneNames.センター].ghost.localPosition += deltaVector;
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.position -= boneGhost.GhostDictionary[BoneNames.センター].ghost.rotation * deltaVector;
                        }
                        else if (nextPositionVMDBoneFrame == null && lastPositionVMDBoneFrame != null)
                        {
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.localPosition -= lastPositionVMDBoneFrame.Position * DefaultBoneAmplifier;
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.position -= boneGhost.GhostDictionary[BoneNames.センター].ghost.rotation * lastPositionVMDBoneFrame.Position * DefaultBoneAmplifier;
                        }
                    }

                    if (lowerBodyVMDBoneFrame != null && lowerBodyVMDBoneFrame.Rotation != ZeroQuaternion)
                    {
                        Quaternion upperBodyRotation = boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation;
                        boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation = boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation.PlusRotation(lowerBodyVMDBoneFrame.Rotation);
                        boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation = upperBodyRotation;
                    }
                    else
                    {
                        if (lowerBodyVMDBoneGroup == null) { return; }
                        VMD.BoneKeyFrame lastRotationVMDBoneFrame = lowerBodyVMDBoneGroup.LastRotationKeyFrame;
                        VMD.BoneKeyFrame nextRotationVMDBoneFrame = lowerBodyVMDBoneGroup.NextRotationKeyFrame;

                        if (nextRotationVMDBoneFrame != null && lastRotationVMDBoneFrame != null)
                        {
                            float rotationInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Rotation, frameNumber, lastRotationVMDBoneFrame.FrameNumber, nextRotationVMDBoneFrame.FrameNumber);
                            Quaternion deltaQuaternion = Quaternion.Lerp(lastRotationVMDBoneFrame.Rotation, nextRotationVMDBoneFrame.Rotation, rotationInterpolationRate);
                            Quaternion upperBodyRotation = boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation;
                            boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation = boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation.PlusRotation(deltaQuaternion);
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation = upperBodyRotation;
                        }
                        else if (lastRotationVMDBoneFrame == null && nextRotationVMDBoneFrame != null)
                        {
                            float rotationInterpolationRate = lowerBodyVMDBoneGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Rotation, frameNumber, 0, nextRotationVMDBoneFrame.FrameNumber);
                            Quaternion deltaQuaternion = Quaternion.Lerp(Quaternion.identity, nextRotationVMDBoneFrame.Rotation, rotationInterpolationRate);
                            Quaternion upperBodyRotation = boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation;
                            boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation = boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation.PlusRotation(deltaQuaternion);
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation = upperBodyRotation;
                        }
                        else if (lastRotationVMDBoneFrame != null && nextRotationVMDBoneFrame == null)
                        {
                            Quaternion upperBodyRotation = boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation;
                            boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation = boneGhost.GhostDictionary[BoneNames.センター].ghost.localRotation.PlusRotation(lastRotationVMDBoneFrame.Rotation);
                            boneGhost.GhostDictionary[BoneNames.上半身].ghost.rotation = upperBodyRotation;
                        }
                    }
                }
            }
        }

        //VMDでは足IKはFootの差分のみの位置、回転情報を持つ
        //また、このコードで足先IKは未実装である
        class FootIK
        {
            public enum Feet { LeftFoot, RightFoot }

            const string TargetName = "IKTarget";

            public VMDReader VMDReader { get; private set; }

            public bool Enable { get; private set; } = true;
            public Feet Foot { get; private set; }
            public Animator Animator { get; private set; }
            public Vector3 Offset { get; private set; }
            public Transform HipTransform { get; private set; }
            public Transform KneeTransform { get; private set; }
            public Transform FootTransform { get; private set; }

            Dictionary<Transform, Quaternion> boneOriginalRotationDictionary;
            Dictionary<Transform, Quaternion> boneOriginalLocalRotationDictionary;

            public Transform Target { get; private set; }
            private BoneNames footBoneName;
            private Vector3 firstLocalPosition;

            private Vector3 firstHipDown;
            private Vector3 firstHipRight;
            private Quaternion firstHipRotation;

            private float upperLegLength = 0;
            private float lowerLegLength = 0;
            private float legLength = 0;
            private float targetDistance = 0;

            public FootIK(VMDReader vmdReader, Animator animator, Feet foot, Vector3 offset)
            {
                VMDReader = vmdReader;
                Foot = foot;
                Animator = animator;
                firstHipDown = -animator.transform.up;
                firstHipRight = animator.transform.right;
                //注意！オフセットのy座標を逆にしている
                Offset = new Vector3(offset.x, -offset.y, offset.z);

                if (Foot == Feet.LeftFoot)
                {
                    footBoneName = BoneNames.左足ＩＫ;
                    HipTransform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    KneeTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    FootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                }
                else
                {
                    footBoneName = BoneNames.右足ＩＫ;
                    HipTransform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    KneeTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    FootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                }

                upperLegLength = Vector3.Distance(HipTransform.position, KneeTransform.position);
                lowerLegLength = Vector3.Distance(KneeTransform.position, FootTransform.position);
                legLength = upperLegLength + lowerLegLength;

                boneOriginalLocalRotationDictionary = new Dictionary<Transform, Quaternion>()
            {
                { HipTransform, HipTransform.localRotation },
                { KneeTransform, KneeTransform.localRotation },
                { FootTransform, FootTransform.localRotation }
            };

                boneOriginalRotationDictionary = new Dictionary<Transform, Quaternion>()
            {
                { HipTransform, HipTransform.rotation },
                { KneeTransform, KneeTransform.rotation },
                { FootTransform, FootTransform.rotation }
            };

                GameObject targetGameObject = new GameObject();
                targetGameObject.transform.position = FootTransform.position;
                targetGameObject.transform.parent = (Animator.GetBoneTransform(HumanBodyBones.Hips).parent);
                Target = targetGameObject.transform;
                Target.name = Foot.ToString() + TargetName;
                firstLocalPosition = Target.localPosition;

                Vector3 targetVector = Target.position - HipTransform.position;
                Vector3 hipDown = HipTransform.rotation * Quaternion.Inverse(boneOriginalRotationDictionary[HipTransform]) * firstHipDown;
                firstHipRotation = Quaternion.AngleAxis(Vector3.Angle(hipDown, targetVector), Vector3.Cross(hipDown, targetVector));
            }

            public void IK()
            {
                upperLegLength = Vector3.Distance(HipTransform.position, KneeTransform.position);
                lowerLegLength = Vector3.Distance(KneeTransform.position, FootTransform.position);
                legLength = upperLegLength + lowerLegLength;

                Vector3 targetVector = Target.position - HipTransform.position;

                targetDistance = Mathf.Min(targetVector.magnitude, legLength);

                float hipAdjacent = ((upperLegLength * upperLegLength) - (lowerLegLength * lowerLegLength) + (targetDistance * targetDistance)) / (2 * targetDistance);
                float hipAngle;
                float hipAngleCos = hipAdjacent / upperLegLength;
                //1や1fではエラー
                if (hipAngleCos > 0.999f) { hipAngle = 0; }
                else if (hipAngleCos < -0.999f) { hipAngle = Mathf.PI * Mathf.Rad2Deg; }
                else { hipAngle = Mathf.Acos(hipAngleCos) * Mathf.Rad2Deg; }

                float kneeAdjacent = ((upperLegLength * upperLegLength) + (lowerLegLength * lowerLegLength) - (targetDistance * targetDistance)) / (2 * lowerLegLength);
                float kneeAngle;
                float kneeAngleCos = kneeAdjacent / upperLegLength;
                //1や1fではエラー
                if (kneeAngleCos > 0.999f) { kneeAngle = 0; }
                else if (kneeAngleCos < -0.999f)
                {
                    kneeAngle = Mathf.PI * Mathf.Rad2Deg;

                    //三角形がつぶれすぎると成立条件が怪しくなりひざの角度が180度になるなど挙動が乱れる
                    if (hipAngle < 0.01f) { kneeAngle = 0; }
                }
                else { kneeAngle = 180 - Mathf.Acos(kneeAdjacent / upperLegLength) * Mathf.Rad2Deg; }

                Vector3 hipDown = HipTransform.rotation * Quaternion.Inverse(boneOriginalRotationDictionary[HipTransform]) * firstHipDown;
                HipTransform.RotateAround(HipTransform.position, Vector3.Cross(hipDown, targetVector), Vector3.Angle(hipDown, targetVector));
                HipTransform.rotation = Quaternion.Inverse(firstHipRotation) * HipTransform.rotation;
                Vector3 hipRight = HipTransform.rotation * Quaternion.Inverse(boneOriginalRotationDictionary[HipTransform]) * firstHipRight;
                HipTransform.RotateAround(HipTransform.position, hipRight, -hipAngle);
                hipRight = HipTransform.rotation * Quaternion.Inverse(boneOriginalRotationDictionary[HipTransform]) * firstHipRight;
                KneeTransform.localRotation = boneOriginalLocalRotationDictionary[KneeTransform];
                KneeTransform.RotateAround(KneeTransform.position, hipRight, kneeAngle);
            }

            public void IK(int frameNumber)
            {
                SetIKEnable(frameNumber);

                if (!Enable) { return; }

                VMD.BoneKeyFrame footIKFrame = VMDReader.GetBoneKeyFrame(footBoneName, frameNumber);

                if (footIKFrame == null || footIKFrame.Position == Vector3.zero) { return; }

                FootTransform.localRotation = boneOriginalLocalRotationDictionary[FootTransform];

                Vector3 moveVector = footIKFrame.Position;

                Target.localPosition = firstLocalPosition + (moveVector * DefaultBoneAmplifier) + Offset;

                IK();
            }

            public void InterpolateIK(int frameNumber)
            {
                SetIKEnable(frameNumber);

                if (!Enable) { return; }

                FootTransform.localRotation = boneOriginalLocalRotationDictionary[FootTransform];

                VMDReader.BoneKeyFrameGroup vmdFootBoneFrameGroup = VMDReader.GetBoneKeyFrameGroup(footBoneName);
                VMD.BoneKeyFrame lastPositionFootVMDBoneFrame = vmdFootBoneFrameGroup.LastPositionKeyFrame;
                VMD.BoneKeyFrame nextPositionFootVMDBoneFrame = vmdFootBoneFrameGroup.NextPositionKeyFrame;

                if (nextPositionFootVMDBoneFrame != null && lastPositionFootVMDBoneFrame != null)
                {
                    float xInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, lastPositionFootVMDBoneFrame.FrameNumber, nextPositionFootVMDBoneFrame.FrameNumber);
                    float yInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, lastPositionFootVMDBoneFrame.FrameNumber, nextPositionFootVMDBoneFrame.FrameNumber);
                    float zInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, lastPositionFootVMDBoneFrame.FrameNumber, nextPositionFootVMDBoneFrame.FrameNumber);

                    float xInterpolation = Mathf.Lerp(lastPositionFootVMDBoneFrame.Position.x, nextPositionFootVMDBoneFrame.Position.x, xInterpolationRate);
                    float yInterpolation = Mathf.Lerp(lastPositionFootVMDBoneFrame.Position.y, nextPositionFootVMDBoneFrame.Position.y, yInterpolationRate);
                    float zInterpolation = Mathf.Lerp(lastPositionFootVMDBoneFrame.Position.z, nextPositionFootVMDBoneFrame.Position.z, zInterpolationRate);

                    Vector3 moveVector = new Vector3(xInterpolation, yInterpolation, zInterpolation);
                    Target.localPosition = firstLocalPosition + (moveVector * DefaultBoneAmplifier) + Offset;
                }
                else if (lastPositionFootVMDBoneFrame == null && nextPositionFootVMDBoneFrame != null)
                {
                    float xInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.X, frameNumber, 0, nextPositionFootVMDBoneFrame.FrameNumber);
                    float yInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Y, frameNumber, 0, nextPositionFootVMDBoneFrame.FrameNumber);
                    float zInterpolationRate = vmdFootBoneFrameGroup.Interpolation.GetInterpolationValue(VMD.BoneKeyFrame.Interpolation.BezierCurveNames.Z, frameNumber, 0, nextPositionFootVMDBoneFrame.FrameNumber);

                    float xInterpolation = Mathf.Lerp(0, nextPositionFootVMDBoneFrame.Position.x, xInterpolationRate);
                    float yInterpolation = Mathf.Lerp(0, nextPositionFootVMDBoneFrame.Position.y, yInterpolationRate);
                    float zInterpolation = Mathf.Lerp(0, nextPositionFootVMDBoneFrame.Position.z, zInterpolationRate);
                    Vector3 moveVector = new Vector3(xInterpolation, yInterpolation, zInterpolation);
                    Target.localPosition = firstLocalPosition + (moveVector * DefaultBoneAmplifier) + Offset;
                }
                else if (nextPositionFootVMDBoneFrame == null && lastPositionFootVMDBoneFrame != null)
                {
                    Target.localPosition = firstLocalPosition + (lastPositionFootVMDBoneFrame.Position * DefaultBoneAmplifier) + Offset;
                }

                IK();
            }

            //使わなくなったIKTargetを削除
            public void Destroy()
            {
                GameObject.Destroy(Target.gameObject);
            }

            //内部でIKのEnableの値を設定している
            private void SetIKEnable(int frame)
            {
                VMD.IKKeyFrame currentIKFrame = VMDReader.RawVMD.IKFrames.Find(x => x.Frame == frame);
                if (currentIKFrame != null)
                {
                    VMD.IKKeyFrame.VMDIKEnable currentIKEnable = currentIKFrame.IKEnable.Find((VMD.IKKeyFrame.VMDIKEnable x) => x.IKName == footBoneName.ToString());
                    if (currentIKEnable != null)
                    {
                        Enable = currentIKEnable.Enable;
                    }
                }
            }
        }

        class ToeIK
        {
            public enum Toes { LeftToe, RightToe }

            const string ToeIKParentName = "IKParent";
            const string FootIKRotatorName = "FootIKRotator";
            const string FootRotatorName = "FootRotator";
            const string ToeIKTargetName = "ToeIKTarget";
            const string FootGhostName = "FootGhost";

            readonly Quaternion ZeroQuaternion = new Quaternion(0, 0, 0, 0);

            public VMDReader VMDReader { get; private set; }
            public BoneGhost BoneGhost { get; private set; }
            public bool FootIKEnable { get; private set; } = true;
            public bool ToeIKEnable { get; private set; } = true;
            public Toes Toe { get; private set; }
            public Animator Animator { get; private set; }
            public Vector3 Offset { get; private set; }
            public Transform FootTransform { get; private set; }
            public Transform ToeTransform { get; private set; }
            public Transform ParentOfAll { get; private set; }

            Quaternion footOriginalLocalRotation = Quaternion.identity;
            //全ての親から見た始めの角度
            Quaternion footOriginalRotation = Quaternion.identity;
            Quaternion toeIKOriginalRotation = Quaternion.identity;
            Vector3 footToeOriginalVector = Vector3.one;

            public Transform ToeIKParent { get; private set; }
            public Transform FootRotator { get; private set; }
            public Transform ToeIKTarget { get; private set; }
            public Transform FootGhost { get; private set; }
            private BoneNames footIKBoneName;
            private BoneNames toeIKBoneName;
            private BoneNames footBoneName;

            public ToeIK(VMDReader vmdReader, Animator animator, BoneGhost boneGhost, Toes toe)
            {
                VMDReader = vmdReader;
                Animator = animator;
                BoneGhost = boneGhost;
                Toe = toe;

                if (Toe == Toes.LeftToe)
                {
                    toeIKBoneName = BoneNames.左つま先ＩＫ;
                    footIKBoneName = BoneNames.左足ＩＫ;
                    footBoneName = BoneNames.左足首;
                    FootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    ToeTransform = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                }
                else
                {
                    toeIKBoneName = BoneNames.右つま先ＩＫ;
                    footIKBoneName = BoneNames.右足ＩＫ;
                    footBoneName = BoneNames.右足首;
                    FootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    ToeTransform = animator.GetBoneTransform(HumanBodyBones.RightToes);
                }

                ParentOfAll = Animator.GetBoneTransform(HumanBodyBones.Hips).parent;

                footOriginalLocalRotation = FootTransform.localRotation;
                footOriginalRotation = FootTransform.rotation;

                footToeOriginalVector = (ToeTransform.position - FootTransform.position);
                footToeOriginalVector.x /= FootTransform.lossyScale.x;
                footToeOriginalVector.y /= FootTransform.lossyScale.y;
                footToeOriginalVector.z /= FootTransform.lossyScale.z;

                GameObject ikParent = new GameObject();
                GameObject footRotator = new GameObject();
                GameObject ikTarget = new GameObject();
                GameObject footGhost = new GameObject();

                ikParent.transform.position = FootTransform.position;
                ikParent.transform.parent = ParentOfAll;
                ToeIKParent = ikParent.transform;
                ToeIKParent.name = Toe.ToString() + ToeIKParentName;

                footRotator.transform.position = FootTransform.position;
                footRotator.transform.parent = ToeIKParent;
                FootRotator = footRotator.transform;
                FootRotator.name = FootRotatorName;

                ikTarget.transform.position = FootTransform.position;
                ikTarget.transform.parent = FootRotator;
                ToeIKTarget = ikTarget.transform;
                ToeIKTarget.name = ToeIKTargetName;

                footGhost.transform.position = FootTransform.position;
                footGhost.transform.parent = ToeIKTarget;
                FootGhost = footGhost.transform;
                FootGhost.name = FootGhostName;
                FootGhost.localRotation = FootTransform.localRotation;
            }

            public void IK()
            {
                FootTransform.localRotation
                    = Quaternion.Inverse(ToeIKParent.rotation) * FootGhost.rotation;
            }

            public void IK(int frameNumber)
            {
                SetIKEnable(frameNumber);

                FootTransform.localRotation = Quaternion.identity;
                ToeIKParent.localRotation = Quaternion.identity;

                VMD.BoneKeyFrame footFrame = VMDReader.GetBoneKeyFrame(footBoneName, frameNumber);
                if (footFrame != null && footFrame.Rotation != ZeroQuaternion)
                {
                    FootRotator.localRotation = footFrame.Rotation;
                }

                if (ToeIKEnable)
                {
                    VMD.BoneKeyFrame toeIKFrame = VMDReader.GetBoneKeyFrame(footIKBoneName, frameNumber);
                    if (toeIKFrame != null)
                    {
                        ToeIKTarget.localRotation = Quaternion.FromToRotation(footToeOriginalVector, footToeOriginalVector + toeIKFrame.Position * DefaultBoneAmplifier);
                    }
                }

                IK();

                if (FootIKEnable)
                {
                    VMD.BoneKeyFrame footIKFrame = VMDReader.GetBoneKeyFrame(footIKBoneName, frameNumber);
                    if (footIKFrame != null && footIKFrame.Rotation != ZeroQuaternion)
                    {
                        Vector3 targetVector = footToeOriginalVector;
                        targetVector = Quaternion.AngleAxis(footIKFrame.Rotation.eulerAngles.x, Vector3.right) * targetVector;
                        targetVector = Quaternion.AngleAxis(footIKFrame.Rotation.eulerAngles.y, Vector3.up) * targetVector;
                        targetVector = Quaternion.AngleAxis(footIKFrame.Rotation.eulerAngles.z, Vector3.forward) * targetVector;

                        Vector3 footToeVector = (ToeTransform.position - FootTransform.position);

                        FootTransform.rotation = Quaternion.FromToRotation(footToeVector, targetVector) * FootTransform.rotation;
                    }
                }
            }

            public void InterpolateIK(int frameNumber)
            {
                SetIKEnable(frameNumber);

                FootTransform.localRotation = Quaternion.identity;
                ToeIKParent.localRotation = Quaternion.identity;

                FootRotator.localRotation
                    = VMDReader.InterporateRotation(VMDReader, footBoneName, frameNumber);

                if (ToeIKEnable)
                {
                    Vector3 moveVector = VMDReader.InterporatePosition(VMDReader, toeIKBoneName, frameNumber);
                    ToeIKTarget.localRotation
                        = Quaternion.FromToRotation(footToeOriginalVector, footToeOriginalVector + moveVector * DefaultBoneAmplifier);
                }

                IK();

                if (FootIKEnable)
                {
                    Quaternion ikQuaternion = VMDReader.InterporateRotation(VMDReader, footIKBoneName, frameNumber);
                    Vector3 targetVector = footToeOriginalVector;
                    targetVector = Quaternion.AngleAxis(ikQuaternion.eulerAngles.x, Vector3.right) * targetVector;
                    targetVector = Quaternion.AngleAxis(ikQuaternion.eulerAngles.y, Vector3.up) * targetVector;
                    targetVector = Quaternion.AngleAxis(ikQuaternion.eulerAngles.z, Vector3.forward) * targetVector;

                    Vector3 footToeVector = (ToeTransform.position - FootTransform.position);

                    FootTransform.rotation = Quaternion.FromToRotation(footToeVector, targetVector) * FootTransform.rotation;
                }
            }

            //使わなくなったIKTargetを削除
            public void Destroy()
            {
                GameObject.Destroy(ToeIKParent.gameObject);
            }

            //内部でIKのEnableの値を設定している
            private void SetIKEnable(int frame)
            {
                VMD.IKKeyFrame currentIKFrame = VMDReader.RawVMD.IKFrames.Find(x => x.Frame == frame);
                if (currentIKFrame != null)
                {
                    VMD.IKKeyFrame.VMDIKEnable currentFootIKEnable = currentIKFrame.IKEnable.Find((VMD.IKKeyFrame.VMDIKEnable x) => x.IKName == footIKBoneName.ToString());
                    if (currentFootIKEnable != null)
                    {
                        FootIKEnable = currentFootIKEnable.Enable;
                    }

                    VMD.IKKeyFrame.VMDIKEnable currentTowIKEnable = currentIKFrame.IKEnable.Find((VMD.IKKeyFrame.VMDIKEnable x) => x.IKName == toeIKBoneName.ToString());
                    if (currentTowIKEnable != null)
                    {
                        ToeIKEnable = currentTowIKEnable.Enable;
                    }
                }
            }
        }

        //裏で正規化されたモデル
        //(初期ポーズで各ボーンのlocalRotationがQuaternion.identityのモデル)を疑似的にアニメーションさせる
        class BoneGhost
        {
            public Dictionary<BoneNames, (Transform ghost, bool enabled)> GhostDictionary { get; private set; } = new Dictionary<BoneNames, (Transform ghost, bool enabled)>();
            public Dictionary<BoneNames, Vector3> OriginalGhostLocalPositionDictionary { get; private set; } = new Dictionary<BoneNames, Vector3>();
            public Dictionary<BoneNames, Quaternion> OriginalRotationDictionary { get; private set; } = new Dictionary<BoneNames, Quaternion>();
            public Dictionary<BoneNames, Quaternion> OriginalGhostRotationDictionary { get; private set; } = new Dictionary<BoneNames, Quaternion>();

            private Dictionary<BoneNames, Transform> boneDictionary = new Dictionary<BoneNames, Transform>();

            public Transform ParentOfAllGhost;
            Transform rootGhost;

            const string ParentOfAllName = "ParentOfAll";
            const string RootName = "Root";
            const string GhostSalt = "Ghost";

            public bool Enabled = true;

            public BoneGhost(Animator animator, Dictionary<BoneNames, Transform> boneDictionary)
            {
                this.boneDictionary = boneDictionary;

                Dictionary<BoneNames, (BoneNames optionParent1, BoneNames optionParent2, BoneNames necessaryParent)> boneParentDictionary
                    = new Dictionary<BoneNames, (BoneNames optionParent1, BoneNames optionParent2, BoneNames necessaryParent)>()
                {
                { BoneNames.センター, (BoneNames.None, BoneNames.None, BoneNames.全ての親) },
                { BoneNames.左足,     (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.左ひざ,   (BoneNames.None, BoneNames.None, BoneNames.左足) },
                { BoneNames.左足首,   (BoneNames.None, BoneNames.None, BoneNames.左ひざ) },
                { BoneNames.右足,     (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.右ひざ,   (BoneNames.None, BoneNames.None, BoneNames.右足) },
                { BoneNames.右足首,   (BoneNames.None, BoneNames.None, BoneNames.右ひざ) },
                { BoneNames.上半身,   (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.上半身2,  (BoneNames.None, BoneNames.None, BoneNames.上半身) },
                { BoneNames.首,       (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.頭,       (BoneNames.首, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.左肩,     (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.左腕,     (BoneNames.左肩, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.左ひじ,   (BoneNames.None, BoneNames.None, BoneNames.左腕) },
                { BoneNames.左手首,   (BoneNames.None, BoneNames.None, BoneNames.左ひじ) },
                { BoneNames.左親指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左親指２, (BoneNames.左親指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指２, (BoneNames.左人指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指３, (BoneNames.左人指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指２, (BoneNames.左中指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指３, (BoneNames.左中指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指２, (BoneNames.左薬指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指３, (BoneNames.左薬指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指２, (BoneNames.左小指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指３, (BoneNames.左小指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右肩,     (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.右腕,     (BoneNames.右肩, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.右ひじ,   (BoneNames.None, BoneNames.None, BoneNames.右腕) },
                { BoneNames.右手首,   (BoneNames.None, BoneNames.None, BoneNames.右ひじ) },
                { BoneNames.右親指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右親指２, (BoneNames.右親指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指２, (BoneNames.右人指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指３, (BoneNames.右人指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指２, (BoneNames.右中指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指３, (BoneNames.右中指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指２, (BoneNames.右薬指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指３, (BoneNames.右薬指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指２, (BoneNames.右小指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指３, (BoneNames.右小指２, BoneNames.None, BoneNames.None) },
                };

                //ParentOfAllGhostとrootGhostの生成
                List<Transform> rootToParentOfAll = new List<Transform>() { boneDictionary[BoneNames.全ての親] };
                Transform rootParent = boneDictionary[BoneNames.全ての親];
                while (rootParent != animator.transform)
                {
                    rootParent = rootParent.parent;
                    rootToParentOfAll.Add(rootParent);
                }
                Dictionary<Transform, Transform> nodeGhostDictionary = new Dictionary<Transform, Transform>();
                foreach (Transform node in rootToParentOfAll)
                {
                    Transform nodeGhost = new GameObject(node.name + GhostSalt).transform;
                    nodeGhost.position = node.position;
                    nodeGhost.rotation = node.rotation;
                    nodeGhostDictionary.Add(node, nodeGhost);
                }
                foreach (Transform node in nodeGhostDictionary.Keys)
                {
                    if (node.parent == null || !nodeGhostDictionary.ContainsKey(node.parent)) { continue; }
                    nodeGhostDictionary[node].parent = nodeGhostDictionary[node.parent];
                }
                ParentOfAllGhost = nodeGhostDictionary[animator.transform];
                ParentOfAllGhost.name = ParentOfAllName + GhostSalt;
                ParentOfAllGhost.parent = animator.transform;
                rootGhost = nodeGhostDictionary[boneDictionary[BoneNames.全ての親]];

                //下位のGhostの生成
                foreach (BoneNames boneName in boneDictionary.Keys)
                {
                    if (boneName == BoneNames.全ての親 || boneName == BoneNames.左足ＩＫ || boneName == BoneNames.右足ＩＫ)
                    {
                        continue;
                    }

                    if (boneDictionary[boneName] == null)
                    {
                        GhostDictionary.Add(boneName, (null, false));
                        continue;
                    }

                    Transform ghost = new GameObject(boneDictionary[boneName].name + GhostSalt).transform;
                    ghost.position = boneDictionary[boneName].position;
                    ghost.rotation = animator.transform.rotation;
                    GhostDictionary.Add(boneName, (ghost, true));
                }

                //下位のGhostの親子構造を設定
                foreach (BoneNames boneName in boneDictionary.Keys)
                {
                    if (boneName == BoneNames.全ての親 || boneName == BoneNames.左足ＩＫ || boneName == BoneNames.右足ＩＫ)
                    {
                        continue;
                    }

                    if (GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled)
                    {
                        continue;
                    }

                    if (boneName == BoneNames.センター)
                    {
                        GhostDictionary[boneName].ghost.SetParent(animator.GetBoneTransform(HumanBodyBones.Hips).parent);
                        continue;
                    }

                    if (boneParentDictionary[boneName].optionParent1 != BoneNames.None && boneDictionary[boneParentDictionary[boneName].optionParent1] != null)
                    {
                        GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].optionParent1].ghost);
                    }
                    else if (boneParentDictionary[boneName].optionParent2 != BoneNames.None && boneDictionary[boneParentDictionary[boneName].optionParent2] != null)
                    {
                        GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].optionParent2].ghost);
                    }
                    else if (boneParentDictionary[boneName].necessaryParent != BoneNames.None && boneDictionary[boneParentDictionary[boneName].necessaryParent] != null)
                    {
                        GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].necessaryParent].ghost);
                    }
                    else
                    {
                        GhostDictionary[boneName] = (GhostDictionary[boneName].ghost, false);
                    }
                }

                foreach (BoneNames boneName in GhostDictionary.Keys)
                {
                    if (GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled)
                    {
                        OriginalGhostLocalPositionDictionary.Add(boneName, Vector3.zero);
                        OriginalGhostRotationDictionary.Add(boneName, Quaternion.identity);
                        OriginalRotationDictionary.Add(boneName, Quaternion.identity);
                    }
                    else
                    {
                        OriginalGhostLocalPositionDictionary.Add(boneName, GhostDictionary[boneName].ghost.localPosition);
                        OriginalGhostRotationDictionary.Add(boneName, GhostDictionary[boneName].ghost.rotation);
                        OriginalRotationDictionary.Add(boneName, boneDictionary[boneName].rotation);
                    }
                }
            }

            public void GhostParentOfAll()
            {
                boneDictionary[BoneNames.全ての親].position = rootGhost.position;

                boneDictionary[BoneNames.全ての親].localRotation
                    = Quaternion.Inverse(boneDictionary[BoneNames.全ての親].parent.rotation)
                    * rootGhost.rotation;
            }

            public void GhostAllChildren()
            {
                foreach (BoneNames boneName in GhostDictionary.Keys)
                {
                    if (boneName == BoneNames.全ての親 || GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled) { continue; }

                    //Ghostを動かした後、実体を動かす
                    boneDictionary[boneName].position = GhostDictionary[boneName].ghost.position;

                    boneDictionary[boneName].localRotation
                        = Quaternion.Inverse(boneDictionary[boneName].parent.rotation)
                        * GhostDictionary[boneName].ghost.rotation
                        * Quaternion.Inverse(OriginalGhostRotationDictionary[boneName])
                        * OriginalRotationDictionary[boneName];
                }
            }

            public void Destroy()
            {
                if (GhostDictionary[BoneNames.センター].ghost == null) { return; }

                GameObject.Destroy(ParentOfAllGhost.gameObject);
                GameObject.Destroy(GhostDictionary[BoneNames.センター].ghost.gameObject);
            }
        }

        class MorphPlayer
        {
            VMDReader vmdReader;
            List<SkinnedMeshRenderer> skinnedMeshRendererList;
            //キーはunity上のモーフ名
            Dictionary<string, MorphDriver> morphDrivers = new Dictionary<string, MorphDriver>();
            //vmd上はまばたきというモーフ名でも、unity上では1.まばたきなどありうるので変換
            //unity上のモーフ名でvmd上のモーフ名を含むものを探す
            Dictionary<string, string> unityVMDMorphNameDictionary = new Dictionary<string, string>();

            int frameNumber = -1;

            public MorphPlayer(Transform model, VMDReader vmdReader)
            {
                this.vmdReader = vmdReader;

                List<SkinnedMeshRenderer> searchBlendShapeSkins(Transform t)
                {
                    List<SkinnedMeshRenderer> skinnedMeshRendererList = new List<SkinnedMeshRenderer>();
                    Queue queue = new Queue();
                    queue.Enqueue(t);
                    while (queue.Count != 0)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = (queue.Peek() as Transform).GetComponent<SkinnedMeshRenderer>();

                        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh.blendShapeCount != 0)
                        {
                            skinnedMeshRendererList.Add(skinnedMeshRenderer);
                        }

                        foreach (Transform childT in (queue.Dequeue() as Transform))
                        {
                            queue.Enqueue(childT);
                        }
                    }

                    return skinnedMeshRendererList;
                }

                skinnedMeshRendererList = searchBlendShapeSkins(model);

                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRendererList)
                {
                    int morphCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
                    for (int i = 0; i < morphCount; i++)
                    {
                        string unityMorphName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                        string vmdMorphName = unityMorphName;
                        //モーフ名に重複があれば2コ目以降は無視
                        if (morphDrivers.Keys.Contains(unityMorphName)) { continue; }
                        //vmd上はまばたきというモーフ名でも、unity上では1.まばたきなどありうるので
                        //unity上のモーフ名でvmd上のモーフ名を含むものを探す
                        if (!vmdReader.FaceKeyFrameGroups.Keys.Contains(unityMorphName))
                        {
                            string[] vmdMorphNames =
                                (from morphName in vmdReader.FaceKeyFrameGroups.Keys where unityMorphName.Contains(morphName) select morphName).ToArray();
                            if (vmdMorphNames == null) { continue; }
                            //0,or2コ以上あるとどれがどれかわからない
                            if (!(vmdMorphNames.Length == 1)) { continue; }
                            vmdMorphName = vmdMorphNames[0];
                        }

                        morphDrivers.Add(unityMorphName, new MorphDriver(skinnedMeshRenderer, i));
                        unityVMDMorphNameDictionary.Add(unityMorphName, vmdMorphName);
                    }
                }
            }

            public void Morph(int frameNumber)
            {
                if (this.frameNumber == frameNumber) { return; }
                foreach (string morphName in morphDrivers.Keys)
                {
                    //含まれないものは除外しているはずだが一応
                    if (!vmdReader.FaceKeyFrameGroups.Keys.Contains(unityVMDMorphNameDictionary[morphName])) { continue; }
                    VMDReader.FaceKeyFrameGroup faceKeyFrameGroup = vmdReader.FaceKeyFrameGroups[unityVMDMorphNameDictionary[morphName]];
                    VMD.FaceKeyFrame faceKeyFrame = faceKeyFrameGroup.GetKeyFrame(frameNumber);
                    if (faceKeyFrame != null)
                    {
                        morphDrivers[morphName].Morph(faceKeyFrame.Weight);
                    }
                    else if (faceKeyFrameGroup.LastMorphKeyFrame != null && faceKeyFrameGroup.NextMorphKeyFrame != null)
                    {
                        float rate =
                            (faceKeyFrameGroup.NextMorphKeyFrame.FrameNumber - frameNumber) * faceKeyFrameGroup.LastMorphKeyFrame.Weight
                            + (frameNumber - faceKeyFrameGroup.LastMorphKeyFrame.FrameNumber) * faceKeyFrameGroup.NextMorphKeyFrame.Weight;
                        rate /= faceKeyFrameGroup.NextMorphKeyFrame.FrameNumber - faceKeyFrameGroup.LastMorphKeyFrame.FrameNumber;
                        morphDrivers[morphName].Morph(rate);
                    }
                    else if (faceKeyFrameGroup.LastMorphKeyFrame != null && faceKeyFrameGroup.NextMorphKeyFrame == null)
                    {
                        morphDrivers[morphName].Morph(faceKeyFrameGroup.LastMorphKeyFrame.Weight);
                    }
                    //全てがnullになることはないはずだが一応
                    else if (faceKeyFrameGroup.LastMorphKeyFrame == null && faceKeyFrameGroup.NextMorphKeyFrame != null)
                    {
                        float rate = faceKeyFrameGroup.NextMorphKeyFrame.Weight * (frameNumber / faceKeyFrameGroup.NextMorphKeyFrame.FrameNumber);
                        morphDrivers[morphName].Morph(rate);
                    }
                }
                this.frameNumber = frameNumber;
            }

            class MorphDriver
            {
                const float MorphAmplifier = 100;

                public SkinnedMeshRenderer SkinnedMeshRenderer { get; private set; }
                public int MorphIndex { get; private set; }

                public MorphDriver(SkinnedMeshRenderer skinnedMeshRenderer, int morphIndex)
                {
                    SkinnedMeshRenderer = skinnedMeshRenderer;
                    MorphIndex = morphIndex;
                }

                public void Morph(float weightRate)
                {
                    SkinnedMeshRenderer.SetBlendShapeWeight(MorphIndex, weightRate * MorphAmplifier);
                }
            }
        }
    }
}
