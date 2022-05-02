using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CameraEffectAction : Action
    {
        private enum State
        {
            SETUP,
            TRANSITION_IN,
            TRANSITION_OUT
        }

        private float transitionDuration;
        private Texture lut;
        private Delegates.InterpolationDelegate interpolation;

        private float clock;
        private float initialContribution;
        private State state;

        private CameraEffectAction() { }

        public CameraEffectAction(string effect, float transitionDuration, Delegates.InterpolationDelegate interpolation)
        {
            Assert.Condition(Props.postProcessing.colorGrading != null, "CameraEffectAction requires a color grading object (and, therefore, a PostProcessVolume component on the main camera)");

            if (effect != null)
            {
                effect = effect.ToLower();
                Assert.Condition(Props.cameraEffects.ContainsKey(effect), string.Format("Unregistered camera effect '{0}'", effect));

                lut = Props.cameraEffects[effect];
            }
            else
            {
                lut = null;
            }

            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            CameraEffectAction clone = new CameraEffectAction();
            clone.transitionDuration = transitionDuration;
            clone.lut = lut;
            clone.interpolation = interpolation;
            clone.clock = clock;
            clone.initialContribution = initialContribution;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch(state)
            {
                case State.SETUP:
                {
                    //Props.postProcessing.colorGrading.active = true;
                    Props.postProcessing.colorGrading.ldrLut.overrideState = true;
                    Props.postProcessing.colorGrading.ldrLutContribution.overrideState = true;

                    if(lut != null)
                    {
                        Props.postProcessing.colorGrading.ldrLut.value = lut;
                        Props.postProcessing.colorGrading.ldrLutContribution.value = 0f;

                        state = State.TRANSITION_IN;
                    }
                    else
                    {
                        initialContribution = Props.postProcessing.colorGrading.ldrLutContribution.value;
                        state = State.TRANSITION_OUT;
                    }

                    break;
                }
                case State.TRANSITION_IN:
                {
                    clock += Time.deltaTime;

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    Props.postProcessing.colorGrading.ldrLutContribution.value = Math.Interpolate(0f, 1f, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    clock += Time.deltaTime;

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    Props.postProcessing.colorGrading.ldrLutContribution.value = Math.Interpolate(initialContribution, 0f, interpolationFactor);

                    if(clock == transitionDuration)
                    {
                        Props.postProcessing.colorGrading.ldrLut.value = null;
                        Props.postProcessing.colorGrading.ldrLut.overrideState = false;
                        //Props.postProcessing.colorGrading.active = false;

                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}