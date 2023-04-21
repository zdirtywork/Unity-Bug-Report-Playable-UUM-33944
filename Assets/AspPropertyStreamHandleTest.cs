using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

// About this issue:
//
// When the weight of AnimationScriptPlayable is not 1, the PropertyStreamHandle within it does not take effect.
// 
// How to reproduce:
// a. Open the "SampleScene".
// b. Enter play mode, then you will see "PSH Value: 100.000" in the Game view.
// c. Drag the weight slider in the Game view.
// d. If the weight is not 1, the "PSH Value" will become 0.000 instead of 100.000*weight.
// 
// Open the PlayableGraph Monitor window from "Tools/Bamboo/PlayableGraph Monitor" to inspect the PlayableGraph.
// 
// PSH means PropertyStreamHandle
// ASP means AnimationScriptPlayable

public struct PshSetJob : IAnimationJob
{
    public PropertyStreamHandle handle;

    public void ProcessRootMotion(AnimationStream stream) => handle.SetFloat(stream, 1);

    public void ProcessAnimation(AnimationStream stream)
    {
    }
}

public struct PshGetJob : IAnimationJob
{
    public PropertyStreamHandle handle;
    public float value;

    public void ProcessRootMotion(AnimationStream stream) => value = handle.GetFloat(stream);

    public void ProcessAnimation(AnimationStream stream)
    {
    }
}

[RequireComponent(typeof(Animator))]
public class AspPropertyStreamHandleTest : MonoBehaviour
{
    public Text pshValueText;
    public AnimationClip clip;
    public string pshName = "PSH_Test";

    private Animator _animator;
    private PlayableGraph _graph;
    private Playable _mixer;
    private AnimationScriptPlayable _pshGetAsp;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _graph = PlayableGraph.Create("PSH Test");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // pshSetAsp → _mixer → _pshGetAsp -> output

        var acp = AnimationClipPlayable.Create(_graph, clip);

        var handle = _animator.BindStreamProperty(_animator.transform, typeof(Animator), pshName);
        var pshSetAsp = AnimationScriptPlayable.Create(_graph, new PshSetJob { handle = handle, });
        pshSetAsp.AddInput(acp, 0, 1f);

        _mixer = AnimationMixerPlayable.Create(_graph);
        _mixer.AddInput(pshSetAsp, 0, 1);

        _pshGetAsp = AnimationScriptPlayable.Create(_graph, new PshGetJob { handle = handle, });
        _pshGetAsp.AddInput(_mixer, 0, 1f);

        var output = AnimationPlayableOutput.Create(_graph, "Anim Output", _animator);
        output.SetSourcePlayable(_pshGetAsp);

        _graph.Play();
    }

    private void LateUpdate() => pshValueText.text = _pshGetAsp.GetJobData<PshGetJob>().value.ToString("F3");
    private void OnDestroy() => _graph.Destroy();

    public void UpdateWeight(float aspWeight) => _mixer.SetInputWeight(0, aspWeight);


#if UNITY_EDITOR
    [ContextMenu("Print Curves")]
    public void PrintCurves()
    {
        if (!clip) return;

        var curves = AnimationUtility.GetCurveBindings(clip);
        foreach (var curve in curves)
        {
            if (string.IsNullOrEmpty(curve.path)) Debug.Log(curve.propertyName);
        }
    }
#endif
}