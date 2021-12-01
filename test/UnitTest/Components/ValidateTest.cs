﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Components;
using BootstrapBlazor.Shared;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UnitTest.Core;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.Components
{
    public class ValidateTest : BootstrapBlazorTestBase
    {
        [Fact]
        public void ChildContent_Ok()
        {
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.AddChildContent("ChildContent-Test");
            });
            Assert.Contains("ChildContent-Test", cut.Markup);
        }

        [Fact]
        public void CascadedEditContext_Ok()
        {
            var model = new Foo() { Name = "Name-Test" };
            Context.RenderTree.Add<CascadingValue<EditContext>>(builder =>
            {
                builder.Add(a => a.Value, new EditContext(model));
            });
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.Value, model.Name);
                builder.Add(a => a.ValueChanged, v => model.Name = v);
                builder.Add(a => a.ValueExpression, model.GenerateValueExpression());
            });
            Assert.Equal(model.Name, cut.Instance.Value);
            cut.Find("input").Change("Test");
            Assert.Equal("Test", model.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void EditorForm_Ok(bool? showLabel)
        {
            var model = new Foo() { Name = "Name-Test" };
            var cut = Context.RenderComponent<EditorForm<Foo>>(builder =>
            {
                builder.Add(a => a.Model, model);
                builder.Add(a => a.ShowLabel, showLabel);
                builder.Add(a => a.AutoGenerateAllItem, false);
#if NET5_0
                builder.Add<EditorItem<string>, Foo>(a => a.FieldItems, f => p =>
#elif NET6_0_OR_GREATER
                builder.Add<EditorItem<Foo, string>, Foo>(a => a.FieldItems, f => p =>
#endif
                {
                    p.Add(p => p.Field, f.Name);
                    p.Add(p => p.FieldExpression, f.GenerateValueExpression());
#if NET5_0
                    p.Add<BootstrapInput<string>, object>(e => e.EditTemplate, f => p =>
#elif NET6_0_OR_GREATER
                    p.Add<BootstrapInput<string>, Foo>(e => e.EditTemplate, f => p =>
#endif
                    {
                        p.Add(a => a.ShowLabel, null);
                        p.Add(a => a.Value, model.Name);
                        p.Add(a => a.ValueExpression, model.GenerateValueExpression());
                    });
                });
            });

            // 内置 EditorForm 时 ShowLabel 为 null 或者 true 时显示标签
            // 内置 EditorForm 时 ShowLabel 为 false 时不显示标签
            if (showLabel == null || showLabel.Value)
            {
                Assert.Contains("label", cut.Markup);
            }
            else
            {
                Assert.DoesNotContain("label", cut.Markup);
            }
        }

        [Fact]
        public void ValidateForm_Ok()
        {
            var model = new Foo() { Name = "Name-Test" };
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(a => a.Model, model);
                builder.AddChildContent<BootstrapInput<string>>(p =>
                {
                    p.Add(a => a.Value, model.Name);
                    p.Add(a => a.ValueExpression, model.GenerateValueExpression());
                });
            });

            // 内置 ValidateForm 验证表单中 ShowLabel 默认 true 显示标签
            var label = cut.Find("label");
            Assert.Equal("姓名", label.InnerHtml);

            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, false);
            });
            Assert.DoesNotContain("label", cut.Markup);
        }

        [Fact]
        public void ShowLabel_Ok()
        {
            var model = new Foo() { Name = "Name-Test" };

            // 显示设置 IsShowLabel=true 时显示标签
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.Value, model.Name);
                builder.Add(a => a.ShowLabel, true);
                builder.Add(a => a.DisplayText, model.Name);
            });
            var label = cut.Find("label");
            Assert.Equal(model.Name, label.InnerHtml);

            // 显示设置 IsShowLabel=false 时不显示标签
            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, false);
            });
            Assert.DoesNotContain("label", cut.Markup);

            // IsShowLabel 为空时 不显示标签
            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, null);
            });
            Assert.DoesNotContain("label", cut.Markup);

            // 开启双向绑定时 IsShowLabel 为空时 不显示标签
            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, null);
                builder.Add(a => a.Value, model.Name);
                builder.Add(a => a.ValueExpression, model.GenerateValueExpression());
            });
            Assert.DoesNotContain("label", cut.Markup);

            // 开启双向绑定时 IsShowLabel=false 时 不显示标签
            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, null);
                builder.Add(a => a.Value, model.Name);
                builder.Add(a => a.ValueExpression, model.GenerateValueExpression());
            });
            Assert.DoesNotContain("label", cut.Markup);

            // 开启双向绑定时 IsShowLabel=true 时 显示标签
            cut.SetParametersAndRender(builder =>
            {
                builder.Add(a => a.ShowLabel, true);
                builder.Add(a => a.Value, model.Name);
                builder.Add(a => a.ValueExpression, model.GenerateValueExpression());
            });
            Assert.Contains("label", cut.Markup);
        }

        [Fact]
        public void SkipValidate_Ok()
        {
            var model = new Foo() { Name = "Name-Test" };
            var valid = false;
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(v => v.Model, model);
                builder.Add(v => v.OnValidSubmit, context =>
                {
                    valid = true;
                    return Task.CompletedTask;
                });
                builder.AddChildContent<BootstrapInput<string>>(pb =>
                {
                    pb.Add(v => v.Value, model.Name);
                    pb.Add(v => v.SkipValidate, true);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression());
                });
                builder.AddChildContent<Button>(pb =>
                {
                    pb.Add(b => b.ButtonType, ButtonType.Submit);
                });
            });

            // 提交表单
            var form = cut.Find("form");
            form.Submit();

            // 内置 ValidateForm 验证表单中 设置 SkipValidate=true 提交表单时不进行验证
            Assert.True(valid);
        }

        [Required]
        private string? Test { get; set; }
        [Fact]
        public void IsRequired_Ok()
        {
            // 组件绑定非公开模型属性
            Test = "test";
            var model = new Foo() { Name = "Name-Test" };
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(a => a.Model, model);
                builder.AddChildContent<BootstrapInput<string>>(p =>
                {
                    p.Add(a => a.Value, Test);
                    p.Add(a => a.ValueChanged, v => Test = v);
                    p.Add(a => a.ValueExpression, Utility.GenerateValueExpression(this, nameof(Test), typeof(string)));
                });
                builder.AddChildContent<BootstrapInput<int>>(p =>
                {
                    p.Add(a => a.Value, model.Count);
                    p.Add(a => a.ValueExpression, model.GenerateValueExpression(nameof(Foo.Count), typeof(int)));
                });
            });
            var input = cut.FindComponent<BootstrapInput<string>>();
            Assert.DoesNotContain("required", input.Markup);

            // 更改值测试
            input.Find("input").Change("test1");
            Assert.Equal(Test, input.Instance.Value);
            Assert.Equal("test1", input.Instance.Value);

            var number = cut.FindComponent<BootstrapInput<int>>();
            Assert.Contains("required=\"true\"", number.Markup);
        }

        [Fact]
        public void SetDisable_Ok()
        {
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.IsDisabled, false);
            });
            Assert.False(cut.Instance.IsDisabled);
            cut.InvokeAsync(() => cut.Instance.SetDisable(true));
            Assert.True(cut.Instance.IsDisabled);
        }

        [Fact]
        public void SetValue_Ok()
        {
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.Value, "test");
            });
            Assert.Equal("test", cut.Instance.Value);
            cut.InvokeAsync(() => cut.Instance.SetValue("test2"));
            Assert.Equal("test2", cut.Instance.Value);
        }

        [Fact]
        public void SetLabel_Ok()
        {
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.DisplayText, "test");
                builder.Add(a => a.ShowLabel, true);
            });
            Assert.Equal("test", cut.Instance.DisplayText);
            cut.InvokeAsync(() => cut.Instance.SetLabel("test1"));
            Assert.Equal("test1", cut.Instance.DisplayText);
        }

        [Fact]
        public void ParsingErrorMessage_Ok()
        {
            var cut = Context.RenderComponent<BootstrapInput<string>>(builder =>
            {
                builder.Add(a => a.ParsingErrorMessage, "test");
            });
            Assert.Equal("test", cut.Instance.ParsingErrorMessage);
        }

        [Fact]
        public void ValidateRules_Ok()
        {
            var model = new Foo() { Name = "test" };
            var invalid = false;
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(v => v.Model, model);
                builder.Add(v => v.OnInvalidSubmit, context =>
                {
                    invalid = true;
                    return Task.CompletedTask;
                });
                builder.AddChildContent<BootstrapInput<string>>(pb =>
                {
                    pb.Add(v => v.Value, model.Name);
                    pb.Add(v => v.ValueChanged, v => model.Name = v);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression());
                });
                builder.AddChildContent<Button>(pb =>
                {
                    pb.Add(b => b.ButtonType, ButtonType.Submit);
                });
            });

            var form = cut.Find("form");
            form.Submit();
            // 提交表单验证通过
            Assert.False(invalid);

            // 设置 Name="" 验证不通过
            var input = cut.FindComponent<BootstrapInput<string>>();
            var c = input.Find("input");
            c.Change("");
            form.Submit();
            Assert.True(invalid);

            // 增加邮箱验证规则
            var rules = new List<IValidator>
            {
                new FormItemValidator(new EmailAddressAttribute())
            };
            input.SetParametersAndRender(pb =>
            {
                pb.Add(v => v.ValidateRules, rules);
            });
            invalid = false;
            c.Change("argo@163.com");
            form.Submit();
            Assert.False(invalid);

            // 更改值不符合邮箱规则验证不通过
            c.Change("argo");
            form.Submit();
            Assert.True(invalid);
        }

        [Fact]
        public void ValidateProperty_Ok()
        {
            var model = new Foo() { Hobby = new string[0] };
            var invalid = false;
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(v => v.Model, model);
                builder.Add(v => v.OnInvalidSubmit, context =>
                {
                    invalid = true;
                    return Task.CompletedTask;
                });
                builder.AddChildContent<CheckboxList<IEnumerable<string>>>(pb =>
                {
                    pb.Add(v => v.Value, model.Hobby);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Hobby), typeof(IEnumerable<string>)));
                    pb.Add(v => v.Items, new List<SelectedItem>()
                    {
                        new SelectedItem("1", "test1"),
                        new SelectedItem("2", "test2")
                    });
                    pb.AddChildContent<Tooltip>();
                });
                builder.AddChildContent<Button>(pb =>
                {
                    pb.Add(b => b.ButtonType, ButtonType.Submit);
                });
            });

            var form = cut.Find("form");
            form.Submit();
            // 提交表单验证不通过
            Assert.True(invalid);

            // 更新选中值
            // 提交表单验证通过
            model.Hobby = new string[] { "1" };
            invalid = false;
            cut.FindComponent<CheckboxList<IEnumerable<string>>>().SetParametersAndRender(pb =>
            {
                pb.Add(v => v.Value, model.Hobby);
            });
            form.Submit();
            Assert.False(invalid);
        }

        [Fact]
        public void CurrentValue_Ok()
        {
            var model = new Foo() { Count = 0 };
            var cut = Context.RenderComponent<RenderTemplate>(builder =>
            {
                builder.AddChildContent<MockValidate<int>>(pb =>
                {
                    pb.Add(v => v.Value, model.Count);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Count), typeof(int)));
                });
                builder.AddChildContent<MockValidate<int?>>();
                builder.AddChildContent<MockValidate<object>>();
            });

            var intNullValidate = cut.FindComponent<MockValidate<int?>>();
            intNullValidate.Instance.Test();
            var intValidate = cut.FindComponent<MockValidate<int>>();
            intValidate.Instance.Test();
            var objValidate = cut.FindComponent<MockValidate<object>>();
            objValidate.Instance.Test();
        }

        [Fact]
        public void CurrentValue_Validate_Ok()
        {
            var model = new Foo();
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(v => v.Model, model);
                builder.AddChildContent<MockValidate<int>>(pb =>
                {
                    pb.Add(v => v.IsDisabled, true);
                    pb.Add(v => v.Value, model.Count);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Count), typeof(int)));
                });
                builder.AddChildContent<MockValidate<int?>>();
                builder.AddChildContent<MockValidate<object>>();
            });

            var intNullValidate = cut.FindComponent<MockValidate<int?>>();
            intNullValidate.Instance.Test();
            var intValidate = cut.FindComponent<MockValidate<int>>();
            intValidate.Instance.Test();
            var objValidate = cut.FindComponent<MockValidate<object>>();
            objValidate.Instance.Test();
        }

        [Fact]
        public void ValidateType_Ok()
        {
            var model = new Foo() { Count = 0 };
            var cut = Context.RenderComponent<RenderTemplate>(builder =>
            {
                builder.AddChildContent<MockValidate<int>>(pb =>
                {
                    pb.Add(v => v.Value, model.Count);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Count), typeof(int)));
                });
            });
            var intValidate = cut.FindComponent<MockValidate<int>>();
            intValidate.Instance.ValidateTypeTest(model);
        }

        [Fact]
        public void OnValidate_Ok()
        {
            var model = new Foo() { Count = 0 };
            var cut = Context.RenderComponent<RenderTemplate>(builder =>
            {
                builder.AddChildContent<MockValidate<int>>(pb =>
                {
                    pb.Add(v => v.Value, model.Count);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Count), typeof(int)));
                });
            });
            var intValidate = cut.FindComponent<MockValidate<int>>();
            intValidate.Instance.OnValidateTest();
        }

        [Fact]
        public void Required_Ok()
        {
            var model = new Foo();
            var rules = new List<IValidator>
            {
                new FormItemValidator(new RequiredAttribute())
            };
            var cut = Context.RenderComponent<ValidateForm>(builder =>
            {
                builder.Add(v => v.Model, model);
                builder.AddChildContent<MockValidate<bool>>(pb =>
                {
                    pb.Add(v => v.Value, model.Complete);
                    pb.Add(v => v.ValueExpression, model.GenerateValueExpression(nameof(Foo.Complete), typeof(bool)));
                    pb.Add(v => v.ValidateRules, rules);
                });
            });

            var boolValidate = cut.FindComponent<MockValidate<bool>>();
            boolValidate.SetParametersAndRender(pb =>
            {
                pb.Add(v => v.ValidateRules, null);
            });
        }

        [Fact]
        public void TooltipHost_Ok()
        {
            var cut = Context.RenderComponent<MockValidate<string>>(builder =>
            {
            });
        }

        private class MockValidate<TValue> : ValidateBase<TValue>
        {
            public void Test()
            {
                CurrentValueAsString = "";
                CurrentValueAsString = "test";
                CurrentValueAsString = "1";
            }

            public void ValidateTypeTest(Foo model)
            {
                CurrentValueAsString = "test";

                var results = new List<ValidationResult>();
                var context = new ValidationContext(model);
                ValidateProperty(1, context, results);
            }

            public void OnValidateTest()
            {
                OnValidate(null);
                OnValidate(false);
                OnValidate(true);
            }
        }
    }
}
