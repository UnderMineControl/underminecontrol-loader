using Octokit;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Core
{
    using Models;
    using System;
    using System.Linq;

    public interface IRepoUtility
    {
        Task<Release> GetRelease(string owner, string repo, WildCard version = null);
    }

    public class RepoUtility : IRepoUtility
    {
        private readonly GitHubClient _client;
        private readonly IConfigUtility _config;

        public RepoUtility(IConfigUtility config)
        {
            _config = config;
            _client = new GitHubClient(new ProductHeaderValue(_config.AppName));
        }

        public async Task<Release> GetRelease(string owner, string repo, WildCard version = null)
        {
            if (version == null)
                return await _client.Repository.Release.GetLatest(owner, repo);

            var all = await _client.Repository.Release.GetAll(owner, repo);
            foreach(var release in all.OrderByDescending(t => t.PublishedAt ?? t.CreatedAt))
            {
                if (version == release.TagName)
                    return release;
            }

            return null;
        }
    }
}
