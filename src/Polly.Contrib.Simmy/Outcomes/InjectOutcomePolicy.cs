﻿using System;
using System.Threading;

namespace Polly.Contrib.Simmy.Outcomes
{
    /// <summary>
    /// A policy that throws an exception in place of executing the passed delegate.
    /// <remarks>The policy can also be configured to return null in place of the exception, to explicitly fake that no exception is thrown.</remarks>
    /// </summary>
    public class InjectOutcomePolicy : MonkeyPolicy
    {
        private readonly Func<Context, CancellationToken, Exception> _faultProvider;

        [Obsolete]
        internal InjectOutcomePolicy(Func<Context, CancellationToken, Exception> faultProvider, Func<Context, CancellationToken, double> injectionRate, Func<Context, CancellationToken, bool> enabled) 
            : base(injectionRate, enabled)
        {
            _faultProvider = faultProvider ?? throw new ArgumentNullException(nameof(faultProvider));
        }

        internal InjectOutcomePolicy(InjectFaultOptions<Exception> options)
            : base(options.InjectionRate, options.Enabled)
        {
            _faultProvider = options.OutcomeInternal ?? throw new ArgumentNullException(nameof(options.OutcomeInternal));
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectExceptionImplementation(
                action,
                context,
                cancellationToken,
                _faultProvider,
                InjectionRate,
                Enabled);
        }
    }

    /// <summary>
    /// A policy that injects an outcome (throws an exception or returns a specific result), in place of executing the passed delegate.
    /// </summary>
    public class InjectOutcomePolicy<TResult> : MonkeyPolicy<TResult>
    {
        private readonly Func<Context, CancellationToken, Exception> _faultProvider;
        private readonly Func<Context, CancellationToken, TResult> _resultProvider;

        [Obsolete]
        internal InjectOutcomePolicy(Func<Context, CancellationToken, Exception> faultProvider, Func<Context, CancellationToken, double> injectionRate, Func<Context, CancellationToken, bool> enabled)
            : base(injectionRate, enabled)
        {
            _faultProvider = faultProvider ?? throw new ArgumentNullException(nameof(faultProvider));
        }

        [Obsolete]
        internal InjectOutcomePolicy(Func<Context, CancellationToken, TResult> resultProvider, Func<Context, CancellationToken, double> injectionRate, Func<Context, CancellationToken, bool> enabled)
            : base(injectionRate, enabled)
        {
            _resultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        }

        internal InjectOutcomePolicy(InjectFaultOptions<Exception> options)
            : base(options.InjectionRate, options.Enabled)
        {
            _faultProvider = options.OutcomeInternal ?? throw new ArgumentNullException(nameof(options.OutcomeInternal));
        }

        internal InjectOutcomePolicy(InjectFaultOptions<TResult> options)
            : base(options.InjectionRate, options.Enabled)
        {
            _resultProvider = options.OutcomeInternal ?? throw new ArgumentNullException(nameof(options.OutcomeInternal));
        }

        /// <inheritdoc/>
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_faultProvider != null)
            {
                return MonkeyEngine.InjectExceptionImplementation(
                    action,
                    context,
                    cancellationToken,
                    _faultProvider,
                    InjectionRate,
                    Enabled);
            }
            else if (_resultProvider != null)
            {
                return MonkeyEngine.InjectResultImplementation(
                    action,
                    context,
                    cancellationToken,
                    _resultProvider,
                    InjectionRate,
                    Enabled);
            }
            else
            {
                throw new InvalidOperationException("Either a fault or fake result to inject must be defined.");
            }
        }
    }
}
