using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using IPTech.DialogManager.Api;
using IPTech.DialogManager;
using NSubstitute;

namespace IPTech.DialogManager {

	public class DialogManagerTest {
	
		IDialog mockDialog1;
		IDialog mockDialog2;
		DialogManagerBasic dialogManagerUnderTest;
		
		public DialogManagerTest() {
			mockDialog1 = Substitute.For<IDialog>();
			mockDialog2 = Substitute.For<IDialog>();
			
			dialogManagerUnderTest = new DialogManagerBasic();
		}
		
		[TestFixture(Description = "EnqueueDialogMethod")]
		public class EnqueueDialogMethod {
			DialogManagerTest fixture;
			
			[SetUp]
			public void SetUp() {
				fixture = new DialogManagerTest();	
			}
			
			[Test]
			public void When_no_other_dialogs_are_enqueued_Calls_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_Does_not_call_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.DidNotReceive().Show(Arg.Any<ShowType>());
			}
			
			[Test]
			public void EnqueuDialog_With_same_dialog_more_than_once_Does_not_call_Show_on_IDialog_a_second_time() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				
				fixture.mockDialog1.Received(1).Show(Arg.Any<ShowType>());
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_Calls_Show_on_IDialog_when_first_dialog_is_closed() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog2);
				
				fixture.mockDialog1.Closed += Raise.Event();
				fixture.mockDialog2.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_ShowDialog_Does_not_call_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.DidNotReceive().Show(Arg.Any<ShowType>());
			}
			
			[Test]
			public void With_the_same_dialog_that_was_enqueued_with_ShowDialog_Does_not_call_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
		}
		
		[TestFixture]
		public class ShowDialogMethod {
		
			DialogManagerTest fixture;
			
			[SetUp]
			public void SetUp() {
				fixture = new DialogManagerTest();	
			}
			
			[Test]
			public void When_no_other_dialogs_are_enqueued_Calls_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_EnqueueDialog_Calls_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_Show_Calls_Show_on_IDialog() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_EnqueueDialog_Calls_Hide_on_enqueued_IDialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog1.Received(1).Hide();
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_Show_Calls_Hide_on_enqueued_IDialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog1.Received(1).Hide();
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_ShowDialog_Calls_Show_on_first_IDialog_when_second_dialog_is_closed() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				fixture.mockDialog2.Closed += Raise.Event();
				
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.RestoreFromHide));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_ShowDialog_Calls_Show_on_new_dialog() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_another_dialog_is_already_enqueued_with_EnqueueDialog_Calls_Show_on_first_IDialog_when_second_dialog_is_closed() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				fixture.mockDialog2.Closed += Raise.Event();
				
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.RestoreFromHide));
			}
			
			[Test]
			public void When_the_same_dialog_is_already_enqueud_with_EnqueueDialog_Calls_Show_on_dialog() {
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.EnqueueDialog(fixture.mockDialog2);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog2);
				
				fixture.mockDialog2.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
			[Test]
			public void When_the_same_dialog_is_already_enqueued_with_ShowDialog_Does_not_call_Show_again() {
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				fixture.dialogManagerUnderTest.ShowDialog(fixture.mockDialog1);
				
				fixture.mockDialog1.Received(1).Show(Arg.Is<ShowType>(ShowType.FirstOpen));
			}
			
		}
	}
}