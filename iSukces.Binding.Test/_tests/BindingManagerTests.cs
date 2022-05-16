using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using iSukces.Binding.Test.Data;
using Xunit;
using Xunit.Abstractions;

namespace iSukces.Binding.Test
{
    public class BindingManagerTests
    {
        public BindingManagerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("dispose listener")]
        [InlineData("dispose binding manager")]
        public void T01_Should_bind_change_and_dispose(string testNumber)
        {
            var testing = new TestingTool(_testOutputHelper);

            var data = new SimpleNpc(testing.log, "Obj");
            data.Title = "initial value";
            Assert.False(data.HasPropertyChangedSubscribers);

            var binding = testing.bm.From(data);
            Assert.False(data.HasPropertyChangedSubscribers);
            binding.WithPath(nameof(data.Title));
            Assert.False(data.HasPropertyChangedSubscribers);
            var listener = binding.CreateListener(info =>
            {
                testing.Listen(info);
            });
            Assert.Equal(1, testing.changessCount);
            Assert.True(data.HasPropertyChangedSubscribers);
            data.Title = "new Value";
            Assert.Equal(2, testing.changessCount);

            if (testNumber == "dispose listener")
                listener.Dispose();
            if (testNumber == "dispose binding manager")
                testing.bm.Dispose();

            Assert.False(data.HasPropertyChangedSubscribers);

            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: 'initial value'
Got value ValueChanged: 'new Value'
Got value EndBinding: 'Unbound', last valid 'new Value'
[Obj] Unubscribe PropertyChanged
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }


        [Fact]
        public void T02_Should_update_source()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data = new SimpleNpc(testing.log, "Obj") { IntNumber = 99 };
            var binding = testing.bm
                .From(data, q => q.IntNumber)
                .WithMode(BindingMode.TwoWay);
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<int>(info =>
            {
                if (info.Value is not BindingSpecial)
                {
                    Assert.True(info.Value is int);
                }

                testing.Listen(info);
            });

            data.IntNumber = 13;
            updater.UpdateSource(27);
            Assert.Equal(27, data.IntNumber);

            Assert.Equal(3, testing.changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
Got value UpdateSource: '27'
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T03_Should_not_update_invalid_value()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { IntNumber = 99 };
            var binding = testing.bm.From(data, q => q.IntNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<int>(info =>
            {
                testing.Listen(info);
            });

            data.IntNumber = 13;
            var updateResult = updater.UpdateSource(DateTime.Now);
            // Assert.True(updateResult.Exception is InvalidCastException);
            Assert.Equal(UpdateSourceResult.InvalidValue, updateResult);
            Assert.Equal(2, testing.changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T04_Should_update_value_with_conversion()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { IntNumber = 99 };
            var binding = testing.bm.From(data, q => q.IntNumber);
            binding.Converter = NumberValueConverter.Instance;
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<string>(info =>
            {
                if (info.Value is not BindingSpecial)
                    Assert.True(info.Value is null or string);

                testing.Listen(info);
            });

            data.IntNumber = 13;
            var updateResult = updater.UpdateSource("27");
            Assert.Equal(UpdateSourceResultStatus.Ok, updateResult.Status);
            Assert.Equal(27, data.IntNumber);

            Assert.Equal(3, testing.changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
Got value UpdateSource: '27'
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }


        [Theory]
        [InlineData("en-US", "$99.45", "$27.44")]
        [InlineData("en-GB", "£99.45", "£27.44")]
        [InlineData("pl-PL", "99,45 zł", "27,44 zł")]
        [InlineData("fr-Fr", "99,45 €", "27,44 €")]
        public void T05_Should_update_value_with_conversion_and_culture(string culture, string expected1,
            string expected2)
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { DecimalNumber = 99.45m };
            var binding = testing.bm.From(data, q => q.DecimalNumber);
            binding.Converter          = NumberValueConverter.Instance;
            binding.CultureInfo        = CultureInfo.GetCultureInfo(culture);
            binding.ConverterParameter = "c2";
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<string>(info =>
            {
                if (info.Value is not BindingSpecial)
                    Assert.True(info.Value is null or string);

                testing.Listen(info);
            });

            var stringNumber = 27.44.ToString(binding.CultureInfo);
            var updateResult = updater.UpdateSource(stringNumber);
            Assert.Equal(UpdateSourceResultStatus.Ok, updateResult.Status);
            Assert.Equal(27.44m, data.DecimalNumber);

            Assert.Equal(2, testing.changessCount);
            var expected = @$"
Try set DecimalNumber=99.45
[Obj] Subscribe PropertyChanged
Got value StartBinding: '{expected1}'
Try set DecimalNumber=27.44
Got value UpdateSource: '{expected2}'";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T06_Should_catch_update_source_exception()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { DecimalNumber = 99.45m };
            var binding = testing.bm.From(data, q => q.DecimalNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<string>(info =>
            {
                if (info.Value is not BindingSpecial)
                {
                    Assert.True(info.Value is string);
                }

                testing.Listen(info);
            });

            var updateResult = updater.UpdateSource(-1.345m);
            Assert.NotNull(updateResult.Exception);
            Assert.Equal(99.45m, data.DecimalNumber);
            Assert.Equal(2, testing.changessCount);
            testing.bm.Dispose();
            const string expected = @"
Try set DecimalNumber=99.45
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99,45'
Try set DecimalNumber=-1.345
Got value UpdateSource: 'Invalid', last valid '99,45'
Got value EndBinding: 'Unbound', last valid '99,45'
[Obj] Unubscribe PropertyChanged
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T07_Should_update_listener_with_dispatcher()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data = new SimpleNpc(testing.log, "Obj") { DecimalNumber = 99.45m };
            var builder = testing.bm
                .From(data, q => q.DecimalNumber);

            _testOutputHelper.WriteLine("Main thread id=" + Thread.CurrentThread.ManagedThreadId);
            var dispatcherCreatedEvent = new ManualResetEventSlim();

            var listenerThreadFinished = new ManualResetEventSlim();
            var dataTheadFinished      = new ManualResetEventSlim();

            var listenerThread = new Thread(() =>
            {
                _testOutputHelper.WriteLine("Listener thread id=" + Thread.CurrentThread.ManagedThreadId);
                testing.WriteLine("Creating dispatcher");
                builder.ListenerDispatcher = Dispatcher.CurrentDispatcher;
                dispatcherCreatedEvent.Set();
                Dispatcher.Run();
                listenerThreadFinished.Set();
                testing.WriteLine("Listener thread finished");
            }) { IsBackground = true };
            var dataThread = new Thread(() =>
            {
                _testOutputHelper.WriteLine("Data thread id=" + Thread.CurrentThread.ManagedThreadId);
                var changingAttempts = 0;
                dispatcherCreatedEvent.Wait();
                Assert.NotEqual(
                    builder.ListenerDispatcher.Thread.ManagedThreadId,
                    Thread.CurrentThread.ManagedThreadId);
                testing.WriteLine("Sure running in two threads");

                builder.CreateListener(info =>
                {
                    changingAttempts++;
                    testing.WriteLine("Changing attempt " + changingAttempts);
                    try
                    {
                        if (builder.ListenerDispatcher.CheckAccess())
                            testing.Listen(info);
                    }
                    finally
                    {
                        if (changingAttempts == 3)
                        {
                            testing.WriteLine("ListenerDispatcher.InvokeShutdown");
                            builder.ListenerDispatcher.InvokeShutdown();
                        }
                    }
                });

                try
                {
                    data.DecimalNumber = 12.34m;
                    testing.log.WriteLine("Set value in main thread");
                }
                catch
                {
                }

                dataTheadFinished.Set();
                testing.WriteLine("Data thread finished");
            }) { IsBackground = true };
            dataThread.Start();
            listenerThread.Start();

            Task.Run(async () =>
            {
                // timeout
                await Task.Delay(10_000);
                testing.WriteLine("timeout");
                listenerThreadFinished.Set();
                dataTheadFinished.Set();
            });
            dataTheadFinished.Wait();
            testing.bm.Dispose();
            //=======
            const string expected = @"
Try set DecimalNumber=99.45
Creating dispatcher
Sure running in two threads
[Obj] Subscribe PropertyChanged
Changing attempt 1
Got value StartBinding: '99,45'
Try set DecimalNumber=12.34
Changing attempt 2
Got value ValueChanged: '12,34'
Set value in main thread
Data thread finished
Changing attempt 3
Got value EndBinding: 'Unbound', last valid '12,34'
ListenerDispatcher.InvokeShutdown
[Obj] Unubscribe PropertyChanged
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
            // cleaning
            builder.ListenerDispatcher.InvokeShutdown();
            listenerThreadFinished.Wait();
        }

        [Fact]
        public void T08_Should_update_with_back_reading()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { DecimalNumber = 99.451m };
            Assert.Equal(99.45m, data.DecimalNumber);
            var binding = testing.bm.From(data, q => q.DecimalNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<string>(info =>
            {
                if (info.Value is not BindingSpecial)
                {
                    Assert.True(info.Value is string);
                }

                testing.Listen(info);
            });

            var updateResult = updater.UpdateSource(99.449m);
            Assert.Null(updateResult.Exception);
            Assert.Equal(99.45m, data.DecimalNumber);
            Assert.Equal(2, testing.changessCount);
            testing.bm.Dispose();
            const string expected = @"
Try set DecimalNumber=99.451
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99,45'
Try set DecimalNumber=99.449
Got value UpdateSource: '99,45'
Got value EndBinding: 'Unbound', last valid '99,45'
[Obj] Unubscribe PropertyChanged
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }
        [Fact]
        public void T09_Should_update_with_back_reading()
        {
            var testing = new TestingTool(_testOutputHelper);

            var data    = new SimpleNpc(testing.log, "Obj") { DecimalNumber = 99.451m };
            Assert.Equal(99.45m, data.DecimalNumber);
            var binding = testing.bm.From(data, q => q.DecimalNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var updater = binding.CreateTwoWayBinding<string>(info =>
            {
                if (info.Value is not BindingSpecial)
                {
                    Assert.True(info.Value is string);
                }

                testing.Listen(info);
            });

            var updateResult = updater.UpdateSource(1234);
            Assert.Null(updateResult.Exception);
            Assert.Equal(1000, data.DecimalNumber);
            Assert.Equal(2, testing.changessCount);
            testing.bm.Dispose();
            const string expected = @"
Try set DecimalNumber=99.451
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99,45'
Try set DecimalNumber=1234
Got value UpdateSource: '1000'
Got value EndBinding: 'Unbound', last valid '1000'
[Obj] Unubscribe PropertyChanged
";
            var log1 = testing.GetLog();
            Assert.Equal(expected.Trim(), log1);
        }

        #region Fields

        private readonly ITestOutputHelper _testOutputHelper;

        #endregion
    }
}
