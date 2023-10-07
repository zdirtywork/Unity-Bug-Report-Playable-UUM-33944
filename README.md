# Unity-Bug-Report-Playable-IN-38805

**Unity has stated that they will not fix this bug.**

> RESOLUTION NOTE:
Thank you for bringing this issue to our attention. Unfortunately, after careful consideration we will not be addressing your issue at this time, as we are currently committed to resolving other higher-priority issues, as well as delivering the new animation system. Our priority levels are determined by factors such as the severity and frequency of an issue and the number of users affected by it. However we know each case is different, so please continue to log any issues you find, as well as provide any general feedback on our roadmap page to help us prioritize.

## About this issue

When the effective weight of `AnimationScriptPlayable` is not `1`, the `PropertyStreamHandle` within it does not take effect.

![Sample](./imgs/img_sample.gif)

## How to reproduce

1. Open the "SampleScene".
2. Enter play mode, then you will see "PSH Value: 100.000" in the Game view.
3. Drag the weight slider in the Game view.
4. If the weight is not `1`, the "PSH Value" will become `0.000` instead of `100.000*weight`.

## Further testing 

If an `AnimationClipPlayable` is added as input to `PshSetJob` and the `PropertyStreamHandle` is bound to a custom property in the AnimationClip, then `PropertyStreamHandle.SetFloat` will modify the property value as expected and be affected by the weight. However, when the weight is not `1`, the modified property value is affected by **the sign** of the original property value. 

![Clip Info](./imgs/img_clipinfo.png)

![Clip Info PSH](./imgs/img_clipinfo_psh.gif)


Open the PlayableGraph Monitor window from "Tools/Bamboo/PlayableGraph Monitor" to inspect the PlayableGraph.

![PlayableGraph Monitor](./imgs/img_playablegraphmonitor.png)

## Solution

This issue can be temporarily fixed by binding a property named `GravityWeight` to the Animator. But I am not sure if doing so will have any other negative effects.

```csharp
_animator.BindCustomStreamProperty("GravityWeight", CustomStreamPropertyType.Float);
```

or

```csharp
_animator.BindStreamProperty(_animator.transform, typeof(Animator), "GravityWeight");
```
