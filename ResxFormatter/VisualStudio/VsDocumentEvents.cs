namespace ResxFormatter.VisualStudio
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;

    /// <summary>
    /// Based on code of "Community toolkit for Visual Studio extensions".
    /// https://github.com/VsixCommunity/Community.VisualStudio.Toolkit
    /// </summary>
    public class VsDocumentEvents : IVsRunningDocTableEvents
    {
        private  RunningDocumentTable Documents { get; }

        public VsDocumentEvents()
        {
            this.Documents = new RunningDocumentTable();
            this.Documents.Advise(this);
        }

        public event EventHandler<VsDocument> Saved;

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            if (Saved != null)
            {
                var info = this.Documents.GetDocumentInfo(docCookie);
                var document = new VsDocument(this.Documents, docCookie, info.Moniker);
                Saved?.Invoke(this, document);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }
    }
}
