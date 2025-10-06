# Time

UT is updated at the beginning of the frame


positions and other values are updated in fixedupdate/physicsupdate to match that UT.
the scene reference frame is updated during physics update as well

reference frame transforms integrating velocities/etc should return positions that match the current scene reference frame (updated in physics update).
Unless the user has manually force-set the value, e.g. `refTrans.Position = pos;` in which case the position should be changed immediately.







