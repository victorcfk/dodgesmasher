//using UnityEngine;

///// <summary>
///// Handler For pausing and resuming unity Animator. Should inherit from an abstract base class.
///// </summary>
//[RequireComponent(typeof(Animation))]
//public class AnimationAddOnAnimator : AnimationAddOn
//{
//	[SerializeField]
//	Animator _animator;
//	AnimatorStateInfo _asi;

//	static string speedFloatName = "speed";
//	static int speedFloatHash = Animator.StringToHash(speedFloatName);

//	protected override void Awake()
//    {
//		if (_animator == null) _animator = GetComponent<Animator>();

//		PondClass.PondPauseEvent += SaveClipInfoBeforePause;
//        PondClass.PondResetEvent += OnPondResetHandler;

//		speedFloatHash = Animator.StringToHash(speedFloatName);
//	}

//	protected override void SaveClipInfoBeforePause(object sender, System.EventArgs e)
//    {
//        if (this.gameObject.activeInHierarchy &&
//			_animator.isActiveAndEnabled &&
//			_animator.isInitialized)
//        {
//			_animator.SetFloat(speedFloatHash, 0);

//			_asi = _animator.GetCurrentAnimatorStateInfo(0);
//			PondClass.PondUnpauseEvent += ResumeAnimation;
//        }
//    }

//	protected override void ResumeAnimation(object sender, System.EventArgs e)
//    {
//        if (gameObject.activeInHierarchy &&
//			_asi.fullPathHash != 0 &&
//			_animator != null)
//        {
//			_animator.SetFloat(speedFloatHash, 1);

//			_animator.Play(_asi.fullPathHash, 0, _asi.normalizedTime);
//			PondClass.PondUnpauseEvent -= ResumeAnimation;

//			ResetValues();
//		}
//    }

//	protected override void ResetValues()
//    {
//		_asi = default(AnimatorStateInfo);
//    }
//}
