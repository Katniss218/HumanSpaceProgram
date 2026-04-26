namespace UnityPlus.PlayerLoop.Phases
{
    /// <summary>
    /// Phase: FrameTiming<br/>
    /// Execution: once per frame (first phase)<br/>
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.TimeUpdate.WaitForLastPresentationAndUpdateTime ) )]
    public struct FrameTiming { }

    /// <summary>
    /// Phase: FrameStart<br/>
    /// Execution: once per frame<br/>
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Time state is finalized.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Frame-scoped state is reset or initialized.<br/>
    /// <br/>
    /// Allowed:<br/>
    /// - Reset state.<br/>
    /// - Schedule frame work.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Consuming simulation results.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.Initialization ) )]
    public struct FrameStart { }

    /// <summary>
    /// Phase: PreFixedUpdate<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.FixedUpdate ), Before = new[] { typeof( FixedUpdate ) } )]
    public struct PreFixedUpdate { }

    /// <summary>
    /// Phase: FixedUpdate<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Inputs are prepared.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Simulation advanced by one timestep.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate ) )]
    public struct FixedUpdate { }

    /// <summary>
    /// Phase: PostFixedUpdate<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - FixedUpdate completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - State is consistent for physics.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Reintroducing FixedUpdate dependencies.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.FixedUpdate ), After = new[] { typeof( FixedUpdate ) }, Before = new[] { typeof( PrePhysicsStep ) } )]
    public struct PostFixedUpdate { }

    /// <summary>
    /// Phase: PrePhysicsStep<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Simulation state is finalized.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Mutating transforms during step.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.FixedUpdate ), After = new[] { typeof( PhysicsStep ) } )]
    public struct PrePhysicsStep { }

    /// <summary>
    /// Phase: PhysicsStep (3D)<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// Typical uses:<br/>
    /// - Custom logic to happen "within" the physics step.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.FixedUpdate.PhysicsFixedUpdate ) )]
    public struct PhysicsStep { }

    /// <summary>
    /// Phase: PhysicsStep (2D)<br/>
    /// Execution: 0..N per frame<br/>
    /// Executes 2D physics.
    /// </summary>
    /// <remarks>
    /// Typical uses:<br/>
    /// - Custom logic to happen "within" the physics step.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.FixedUpdate.Physics2DFixedUpdate ) )]
    public struct PhysicsStep2D { }

    /// <summary>
    /// Phase: PostPhysicsStep<br/>
    /// Execution: 0..N per frame<br/>
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - All physics completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - State reflects final physics results.<br/>
    /// <br/>
    /// Typical uses:<br/>
    /// - Read physics outputs.<br/>
    /// - Apply corrections/validation.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.FixedUpdate ), After = new[] { typeof( PhysicsStep2D ) } )]
    public struct PostPhysicsStep { }

    /// <summary>
    /// Phase: PreUpdate<br/>
    /// Execution: once per frame<br/>
    /// Prepares frame-level logic.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Fixed-time steps completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Inputs and state prepared.<br/>
    /// <br/>
    /// Allowed:<br/>
    /// - Process input.<br/>
    /// - Sync systems.<br/>
    /// <br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Update ), Before = new[] { typeof( Update ) } )]
    public struct PreUpdate { }

    /// <summary>
    /// Phase: Update<br/>
    /// Execution: once per frame<br/>
    /// Advances frame-dependent logic.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Inputs prepared.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Frame logic advanced.<br/>
    /// <br/>
    /// Allowed:<br/>
    /// - Mutate frame-dependent state.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Deterministic physics logic.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate ) )]
    public struct Update { }

    /// <summary>
    /// Phase: PostUpdate<br/>
    /// Execution: once per frame<br/>
    /// Finalizes Update results.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Update completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - State ready for LateUpdate.<br/>
    /// <br/>
    /// Allowed:<br/>
    /// - Apply deferred changes.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Reintroducing Update dependencies.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Update ), After = new[] { typeof( Update ) } )]
    public struct PostUpdate { }

    /// <summary>
    /// Phase: PreLateUpdate<br/>
    /// Execution: once per frame<br/>
    /// Prepares late-stage systems.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Update completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - State ready for LateUpdate.<br/>
    /// <br/>
    /// Allowed:<br/>
    /// - Prepare dependent systems.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PreLateUpdate ), Before = new[] { typeof( LateUpdate ) } )]
    public struct PreLateUpdate { }

    /// <summary>
    /// Phase: LateUpdate<br/>
    /// Execution: once per frame<br/>
    /// Resolves late dependencies.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Update completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Late-dependent systems resolved.<br/>
    /// </remarks>
    [PlayerLoopNative( Alias = typeof( UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate ) )]
    public struct LateUpdate { }

    /// <summary>
    /// Phase: PostLateUpdate<br/>
    /// Execution: once per frame<br/>
    /// Finalizes state before rendering.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - LateUpdate completed.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - State stable for rendering.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Mutating render state.<br/>
    /// - Reintroducing LateUpdate dependencies.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PreLateUpdate ), After = new[] { typeof( LateUpdate ) } )]
    public struct PostLateUpdate { }

    /// <summary>
    /// Phase: PreRender<br/>
    /// Execution: once per frame<br/>
    /// Prepares rendering inputs.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Simulation complete.<br/>
    /// Typical use:<br/>
    /// - Sync simulation to render state.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PostLateUpdate ), Before = new[] { typeof( UnityEngine.PlayerLoop.PostLateUpdate.UpdateRectTransform ) } )]
    public struct PreRender { }

    /// <summary>
    /// Phase: Render<br/>
    /// Execution: once per frame<br/>
    /// Submits rendering work.
    /// </summary>
    /// <remarks>
    /// Typical use:<br/>
    /// - Rendering stuff.<br/>
    /// <br/>
    /// Prohibited:<br/>
    /// - Mutating simulation state.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PostLateUpdate ), After = new[] { typeof( UnityEngine.PlayerLoop.PostLateUpdate.UpdateAllRenderers ) }, Before = new[] { typeof( UnityEngine.PlayerLoop.PostLateUpdate.FinishFrameRendering ) } )]
    public struct Render { }

    /// <summary>
    /// Phase: PostRender<br/>
    /// Execution: once per frame<br/>
    /// Handles post-render work.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - Rendering submitted.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Frame ready for finalization.<br/>
    /// <br/>
    /// Typical use:<br/>
    /// - GPU readbacks.<br/>
    /// - Diagnostics.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PostLateUpdate ), After = new[] { typeof( UnityEngine.PlayerLoop.PostLateUpdate.FinishFrameRendering ) } )]
    public struct PostRender { }

    /// <summary>
    /// Phase: FrameEnd<br/>
    /// Execution: once per frame (final phase)<br/>
    /// Finalizes frame.
    /// </summary>
    /// <remarks>
    /// Preconditions:<br/>
    /// - All rendering complete.<br/>
    /// <br/>
    /// Postconditions:<br/>
    /// - Frame fully finalized.<br/>
    /// </remarks>
    [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.PostLateUpdate ), After = new[] { typeof( UnityEngine.PlayerLoop.PostLateUpdate.TriggerEndOfFrameCallbacks ) } )]
    public struct FrameEnd { }
}