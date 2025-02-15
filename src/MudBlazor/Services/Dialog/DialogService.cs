﻿// Copyright (c) 2019 Blazored (https://github.com/Blazored)
// See https://github.com/Blazored
// License: MIT
// Copyright (c) 2020 Adapted by MudBlazor

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

#nullable enable
namespace MudBlazor
{
    /// <summary>
    /// A service for managing <see cref="MudDialog"/> components.
    /// </summary>
    /// <remarks>
    /// This service requires a <see cref="MudDialogProvider"/> in your layout page.
    /// </remarks>
    /// <seealso cref="MudDialog"/>
    /// <seealso cref="MudDialogContainer"/>
    /// <seealso cref="MudDialogProvider"/>
    /// <seealso cref="DialogOptions"/>
    /// <seealso cref="DialogParameters{T}"/>
    /// <seealso cref="DialogReference"/>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// This internal wrapper components prevents overwriting parameters of once
        /// instantiated dialog instances
        /// See: https://github.com/MudBlazor/MudBlazor/issues/10659#issuecomment-2602911059
        /// </summary>
        private class DialogHelperComponent : IComponent
        {
            private const string ChildContent = nameof(ChildContent);
            private RenderFragment? _renderFragment;
            private RenderHandle _renderHandle;
            void IComponent.Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

            Task IComponent.SetParametersAsync(ParameterView parameters)
            {
                if (_renderFragment is null && parameters.TryGetValue<RenderFragment>(ChildContent, out var renderFragment))
                {
                    _renderFragment = renderFragment;
                    _renderHandle.Render(_renderFragment);
                }

                return Task.CompletedTask;
            }

            public static RenderFragment Wrap(RenderFragment renderFragment)
                => builder =>
                {
                    builder.OpenComponent<DialogHelperComponent>(1);
                    builder.AddAttribute(2, ChildContent, renderFragment);
                    builder.CloseComponent();
                };
        }

        /// <inheritdoc />
        public event Func<IDialogReference, Task>? DialogInstanceAddedAsync;

        /// <inheritdoc />
        public event Action<IDialogReference, DialogResult?>? OnDialogCloseRequested;

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : IComponent
        {
            return Show<T>(string.Empty, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title) where T : IComponent
        {
            return Show<T>(title, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogOptions options) where T : IComponent
        {
            return Show<T>(title, DialogParameters.Default, options);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogParameters parameters) where T : IComponent
        {
            return Show<T>(title, parameters, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogParameters parameters, DialogOptions? options)
            where T : IComponent
        {
            return Show(typeof(T), title, parameters, options ?? DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent)
        {
            return Show(contentComponent, string.Empty, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title)
        {
            return Show(contentComponent, title, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogOptions options)
        {
            return Show(contentComponent, title, DialogParameters.Default, options);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogParameters parameters)
        {
            return Show(contentComponent, title, parameters, DialogOptions.Default);
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync instead. This will be removed in future major version.")]
        public IDialogReference Show([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogParameters parameters,
            DialogOptions options)
        {
            if (!typeof(IComponent).IsAssignableFrom(contentComponent))
            {
                throw new ArgumentException($"{contentComponent.FullName} must be a Blazor IComponent");
            }

            var dialogReference = CreateReference();

            var dialogContent = DialogHelperComponent.Wrap(builder =>
            {
                var i = 0;
                builder.OpenComponent(i++, contentComponent);
                foreach (var parameter in parameters)
                {
                    builder.AddAttribute(i++, parameter.Key, parameter.Value);
                }

                builder.AddComponentReferenceCapture(i, inst => { dialogReference.InjectDialog(inst); });
                builder.CloseComponent();
            });
            var dialogInstance = new RenderFragment(builder =>
            {
                builder.OpenComponent<MudDialogContainer>(0);
                builder.SetKey(dialogReference.Id);
                builder.AddComponentParameter(1, nameof(MudDialogContainer.Options), options);
                builder.AddComponentParameter(2, nameof(MudDialogContainer.Title), title);
                builder.AddComponentParameter(3, nameof(MudDialogContainer.Content), dialogContent);
                builder.AddComponentParameter(4, nameof(MudDialogContainer.Id), dialogReference.Id);
                builder.CloseComponent();
            });
            dialogReference.InjectRenderFragment(dialogInstance);
            DialogInstanceAddedAsync?.Invoke(dialogReference);

            return dialogReference;
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : IComponent
        {
            return ShowAsync<T>(string.Empty, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title) where T : IComponent
        {
            return ShowAsync<T>(title, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogOptions options) where T : IComponent
        {
            return ShowAsync<T>(title, DialogParameters.Default, options);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogParameters parameters) where T : IComponent
        {
            return ShowAsync<T>(title, parameters, DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? title, DialogParameters parameters,
            DialogOptions? options) where T : IComponent
        {
            return ShowAsync(typeof(T), title, parameters, options ?? DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent)
        {
            return ShowAsync(contentComponent, string.Empty, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title)
        {
            return ShowAsync(contentComponent, title, DialogParameters.Default, DialogOptions.Default);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogOptions options)
        {
            return ShowAsync(contentComponent, title, DialogParameters.Default, options);
        }

        /// <inheritdoc />
        public Task<IDialogReference> ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogParameters parameters)
        {
            return ShowAsync(contentComponent, title, parameters, DialogOptions.Default);
        }

        /// <inheritdoc />
        public async Task<IDialogReference> ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title,
            DialogParameters parameters, DialogOptions options)
        {
            var dialogReference = await ShowCoreAsync(contentComponent, title, parameters, options);

            //Do not wait forever, what if render fails because of some internal exception and we will never release the method.
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var token = cancellationTokenSource.Token;
            await using (token.Register(() => dialogReference.RenderCompleteTaskCompletionSource.TrySetResult(false)))
            {
                await dialogReference.RenderCompleteTaskCompletionSource.Task;

                return dialogReference;
            }
        }

        /// <inheritdoc />
        public Task<bool?> ShowMessageBox(string? title, string message, string yesText = "OK",
            string? noText = null, string? cancelText = null, DialogOptions? options = null)
        {
            return ShowMessageBox(new MessageBoxOptions
            {
                Title = title,
                Message = message,
                YesText = yesText,
                NoText = noText,
                CancelText = cancelText,
            }, options);
        }

        /// <inheritdoc />
        public Task<bool?> ShowMessageBox(string? title, MarkupString markupMessage, string yesText = "OK",
            string? noText = null, string? cancelText = null, DialogOptions? options = null)
        {
            return ShowMessageBox(new MessageBoxOptions
            {
                Title = title,
                MarkupMessage = markupMessage,
                YesText = yesText,
                NoText = noText,
                CancelText = cancelText,
            }, options);
        }

        /// <inheritdoc />
        public async Task<bool?> ShowMessageBox(MessageBoxOptions messageBoxOptions, DialogOptions? options = null)
        {
            var parameters = new DialogParameters
            {
                [nameof(MessageBoxOptions.Title)] = messageBoxOptions.Title,
                [nameof(MessageBoxOptions.Message)] = messageBoxOptions.Message,
                [nameof(MessageBoxOptions.MarkupMessage)] = messageBoxOptions.MarkupMessage,
                [nameof(MessageBoxOptions.CancelText)] = messageBoxOptions.CancelText,
                [nameof(MessageBoxOptions.NoText)] = messageBoxOptions.NoText,
                [nameof(MessageBoxOptions.YesText)] = messageBoxOptions.YesText,
            };
            var reference = await ShowAsync<MudMessageBox>(title: messageBoxOptions.Title, parameters: parameters, options: options);
            var result = await reference.Result;

            if (result is null)
            {
                return null;
            }

            if (result.Canceled || result.Data is not bool data)
            {
                return null;
            }

            return data;
        }

        /// <inheritdoc />
        public void Close(IDialogReference dialog)
        {
            Close(dialog, DialogResult.Ok<object?>(null));
        }

        /// <inheritdoc />
        public virtual void Close(IDialogReference dialog, DialogResult? result)
        {
            OnDialogCloseRequested?.Invoke(dialog, result);
        }

        /// <inheritdoc />
        public virtual IDialogReference CreateReference()
        {
            return new DialogReference(Guid.NewGuid(), this);
        }

        private async Task<IDialogReference> ShowCoreAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type contentComponent, string? title, DialogParameters parameters,
            DialogOptions options)
        {
            if (!typeof(IComponent).IsAssignableFrom(contentComponent))
            {
                throw new ArgumentException($"{contentComponent.FullName} must be a Blazor IComponent");
            }

            var dialogReference = CreateReference();

            var dialogContent = DialogHelperComponent.Wrap(builder =>
            {
                var i = 0;
                builder.OpenComponent(i++, contentComponent);
                foreach (var parameter in parameters)
                {
                    builder.AddAttribute(i++, parameter.Key, parameter.Value);
                }

                builder.AddComponentReferenceCapture(i, inst => { dialogReference.InjectDialog(inst); });
                builder.CloseComponent();
            });
            var dialogInstance = new RenderFragment(builder =>
            {
                builder.OpenComponent<MudDialogContainer>(0);
                builder.SetKey(dialogReference.Id);
                builder.AddComponentParameter(1, nameof(MudDialogContainer.Options), options);
                builder.AddComponentParameter(2, nameof(MudDialogContainer.Title), title);
                builder.AddComponentParameter(3, nameof(MudDialogContainer.Content), dialogContent);
                builder.AddComponentParameter(4, nameof(MudDialogContainer.Id), dialogReference.Id);
                builder.CloseComponent();
            });
            dialogReference.InjectRenderFragment(dialogInstance);

            var dialogInstanceAddedAsync = DialogInstanceAddedAsync;
            if (dialogInstanceAddedAsync is not null)
            {
                await dialogInstanceAddedAsync(dialogReference);
            }

            return dialogReference;
        }
    }
}
